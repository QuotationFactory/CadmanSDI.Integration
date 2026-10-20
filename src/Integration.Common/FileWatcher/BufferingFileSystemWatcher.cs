#nullable disable
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Integration.Common.FileWatcher;

/// <devdoc>
/// Features:
/// - Buffers FileSystemWatcher events in a BlockinCollection to prevent InternalBufferOverflowExceptions.
/// - Does not break the original FileSystemWatcher API.
/// - Supports reporting existing files via a new Existed event.
/// - Supports sorting events by oldest (existing) file first.
/// - Supports an new event Any reporting any FSW change.
/// - Offers the Error event in Win Forms designer (via [Browsable[true)]
/// - Does not prevent duplicate files occuring.
/// Notes:
///   We contain FilSystemWatcher to follow the principle of composition over inheritance
///   and because System.IO.FileSystemWatcher is not designed to be inherited from:
///   Event handlers and Dispose(disposing) are not virtual.
/// </devdoc>
public class BufferingFileSystemWatcher : Component
{
    private readonly System.IO.FileSystemWatcher _containedFsw;
    private readonly object _eventLock = new();
    private readonly object _lifecycleLock = new();
    private bool _createdSubscribed;
    private bool _changedSubscribed;
    private bool _deletedSubscribed;
    private bool _renamedSubscribed;

    private FileSystemEventHandler _onExistedHandler;
    private FileSystemEventHandler _onAllChangesHandler;

    private FileSystemEventHandler _onCreatedHandler;
    private FileSystemEventHandler _onChangedHandler;
    private FileSystemEventHandler _onDeletedHandler;
    private RenamedEventHandler _onRenamedHandler;

    private ErrorEventHandler _onErrorHandler;

    //We use a single buffer for all change types. Alternatively we could use one buffer per event type, costing additional enumerate tasks.
    private EventPump _activePump;

    #region Contained FileSystemWatcher
    public BufferingFileSystemWatcher()
    {
        _containedFsw = new System.IO.FileSystemWatcher();
    }

    public BufferingFileSystemWatcher(string path)
    {
        _containedFsw = new System.IO.FileSystemWatcher(path, "*.*");
    }

    public BufferingFileSystemWatcher(string path, string filter)
    {
        _containedFsw = new System.IO.FileSystemWatcher(path, filter);
    }

    public bool EnableRaisingEvents
    {
        get
        {
            return _containedFsw.EnableRaisingEvents;
        }
        set
        {
            lock (_lifecycleLock)
            {
                if (_containedFsw.EnableRaisingEvents == value) return;

                if (value)
                {
                    var pump = new EventPump(EventQueueCapacity);
                    _activePump = pump;
                    try
                    {
                        // Enable first so changes during the existing-file scan are also buffered.
                        _containedFsw.EnableRaisingEvents = true;
                        RaiseBufferedEventsUntilCancelled(pump);
                    }
                    catch
                    {
                        _containedFsw.EnableRaisingEvents = false;
                        _activePump = null;
                        pump.Cancel();
                        pump.Dispose();
                        throw;
                    }
                }
                else
                {
                    _containedFsw.EnableRaisingEvents = false;
                    var pump = _activePump;
                    _activePump = null;
                    pump?.Cancel();
                }
            }
        }
    }

    public string Filter
    {
        get { return _containedFsw.Filter; }
        set { _containedFsw.Filter = value; }
    }

    public Collection<string> Filters => _containedFsw.Filters;

    public bool IncludeSubdirectories
    {
        get { return _containedFsw.IncludeSubdirectories; }
        set { _containedFsw.IncludeSubdirectories = value; }
    }

    public int InternalBufferSize
    {
        get { return _containedFsw.InternalBufferSize; }
        set { _containedFsw.InternalBufferSize = value; }
    }

    public NotifyFilters NotifyFilter
    {
        get { return _containedFsw.NotifyFilter; }
        set { _containedFsw.NotifyFilter = value; }
    }

    public string Path
    {
        get { return _containedFsw.Path; }
        set { _containedFsw.Path = value; }
    }

    public ISynchronizeInvoke SynchronizingObject
    {
        get { return _containedFsw.SynchronizingObject; }
        set { _containedFsw.SynchronizingObject = value; }
    }

    public override ISite Site
    {
        get { return _containedFsw.Site; }
        set { _containedFsw.Site = value; }
    }

    #endregion

    [DefaultValue(false)]
    public bool OrderByOldestFirst { get; set; } = false;

    public int EventQueueCapacity { get; set; } = int.MaxValue;

    #region New BufferingFileSystemWatcher specific events
    public event FileSystemEventHandler Existed
    {
        add
        {
            lock (_eventLock)
                _onExistedHandler += value;
        }
        remove
        {
            lock (_eventLock)
                _onExistedHandler -= value;
        }
    }

    public event FileSystemEventHandler All
    {
        add
        {
            lock (_eventLock)
            {
                _onAllChangesHandler += value;
                UpdateUnderlyingSubscriptions();
            }
        }
        remove
        {
            lock (_eventLock)
            {
                _onAllChangesHandler -= value;
                UpdateUnderlyingSubscriptions();
            }
        }
    }

    #endregion

    #region Standard FSW events
    //- The _fsw events add to the buffer.
    //- The public events raise from the buffer to the consumer.
    public event FileSystemEventHandler Created
    {
        add
        {
            lock (_eventLock)
            {
                _onCreatedHandler += value;
                UpdateUnderlyingSubscriptions();
            }
        }
        remove
        {
            lock (_eventLock)
            {
                _onCreatedHandler -= value;
                UpdateUnderlyingSubscriptions();
            }
        }
    }

    public event FileSystemEventHandler Changed
    {
        add
        {
            lock (_eventLock)
            {
                _onChangedHandler += value;
                UpdateUnderlyingSubscriptions();
            }
        }
        remove
        {
            lock (_eventLock)
            {
                _onChangedHandler -= value;
                UpdateUnderlyingSubscriptions();
            }
        }
    }

    public event FileSystemEventHandler Deleted
    {
        add
        {
            lock (_eventLock)
            {
                _onDeletedHandler += value;
                UpdateUnderlyingSubscriptions();
            }
        }
        remove
        {
            lock (_eventLock)
            {
                _onDeletedHandler -= value;
                UpdateUnderlyingSubscriptions();
            }
        }
    }

    public event RenamedEventHandler Renamed
    {
        add
        {
            lock (_eventLock)
            {
                _onRenamedHandler += value;
                UpdateUnderlyingSubscriptions();
            }
        }
        remove
        {
            lock (_eventLock)
            {
                _onRenamedHandler -= value;
                UpdateUnderlyingSubscriptions();
            }
        }
    }

    private void BufferEvent(object _, FileSystemEventArgs e)
    {
        EventPump pump;
        lock (_lifecycleLock)
            pump = _activePump;

        if (pump != null && !pump.TryAdd(e))
        {
            ReportError(new EventQueueOverflowException($"Event queue size {pump.Capacity} events exceeded."));
        }
    }

    private void UpdateUnderlyingSubscriptions()
    {
        var all = _onAllChangesHandler != null;
        SetSubscription(ref _createdSubscribed, all || _onCreatedHandler != null,
            () => _containedFsw.Created += BufferEvent, () => _containedFsw.Created -= BufferEvent);
        SetSubscription(ref _changedSubscribed, all || _onChangedHandler != null,
            () => _containedFsw.Changed += BufferEvent, () => _containedFsw.Changed -= BufferEvent);
        SetSubscription(ref _deletedSubscribed, all || _onDeletedHandler != null,
            () => _containedFsw.Deleted += BufferEvent, () => _containedFsw.Deleted -= BufferEvent);
        SetSubscription(ref _renamedSubscribed, all || _onRenamedHandler != null,
            () => _containedFsw.Renamed += BufferEvent, () => _containedFsw.Renamed -= BufferEvent);
    }

    private static void SetSubscription(ref bool subscribed, bool shouldSubscribe, Action subscribe, Action unsubscribe)
    {
        if (subscribed == shouldSubscribe) return;

        if (shouldSubscribe)
            subscribe();
        else
            unsubscribe();

        subscribed = shouldSubscribe;
    }

    public event ErrorEventHandler Error
    {
        add
        {
            lock (_eventLock)
            {
                var wasUnsubscribed = _onErrorHandler == null;
                _onErrorHandler += value;
                if (wasUnsubscribed && _onErrorHandler != null)
                    _containedFsw.Error += BufferingFileSystemWatcher_Error;
            }
        }
        remove
        {
            lock (_eventLock)
            {
                _onErrorHandler -= value;
                if (_onErrorHandler == null)
                    _containedFsw.Error -= BufferingFileSystemWatcher_Error;
            }
        }
    }

    private void BufferingFileSystemWatcher_Error(object sender, ErrorEventArgs e)
    {
        ReportError(e.GetException());
    }
    #endregion

    private void RaiseBufferedEventsUntilCancelled(EventPump pump)
    {
        _ = Task.Run(() =>
        {
            try
            {
                if (!pump.Token.IsCancellationRequested && (_onExistedHandler != null || _onAllChangesHandler != null))
                {
                    try
                    {
                        NotifyExistingFiles(pump.Token);
                    }
                    catch (OperationCanceledException) when (pump.Token.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        ReportError(ex);
                    }
                }

                foreach (
                    FileSystemEventArgs e in
                    pump.Events.GetConsumingEnumerable(pump.Token))
                {
                    DispatchBufferedEvent(e);
                }
            }
            catch (OperationCanceledException) when (pump.Token.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
            finally
            {
                pump.Dispose();
            }
        });
    }

    private void DispatchBufferedEvent(FileSystemEventArgs e)
    {
        InvokeHandler(_onAllChangesHandler, e);

        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Created:
                InvokeHandler(_onCreatedHandler, e);
                break;
            case WatcherChangeTypes.Changed:
                InvokeHandler(_onChangedHandler, e);
                break;
            case WatcherChangeTypes.Deleted:
                InvokeHandler(_onDeletedHandler, e);
                break;
            case WatcherChangeTypes.Renamed:
                InvokeHandler(_onRenamedHandler, e as RenamedEventArgs);
                break;
        }
    }

    private void NotifyExistingFiles(CancellationToken cancellationToken)
    {
        var directoryInfo = new DirectoryInfo(Path);
        var searchOption = IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        var existingFiles = GetAllFilters().SelectMany(x => directoryInfo.GetFiles(x, searchOption));

        if (OrderByOldestFirst)
            existingFiles = existingFiles.OrderBy(x => x.LastWriteTime);

        foreach (var fileInfo in existingFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            InvokeHandler(_onExistedHandler, new FileSystemEventArgs(WatcherChangeTypes.All, fileInfo.DirectoryName ?? Path, fileInfo.Name));
            InvokeHandler(_onAllChangesHandler, new FileSystemEventArgs(WatcherChangeTypes.All, fileInfo.DirectoryName ?? Path, fileInfo.Name));
        }
    }

    public IEnumerable<string> GetAllFilters()
    {
        var allFilters = Filters?.ToList() ?? new List<string>();
        if (allFilters.Count == 0 && !string.IsNullOrEmpty(Filter))
            allFilters.Add(Filter);

        if (allFilters.Count == 0)
            throw new ArgumentNullException("Filter(s) are not set.");

        return allFilters;
    }

    #region InvokeHandlers
    //Automatically raise event in calling thread when _fsw.SynchronizingObject is set. Ex: When used as a component in Win Forms.
    private void InvokeHandler(FileSystemEventHandler eventHandler, FileSystemEventArgs e)
    {
        if (eventHandler == null) return;

        foreach (FileSystemEventHandler handler in eventHandler.GetInvocationList())
        {
            try
            {
                if (_containedFsw.SynchronizingObject != null && _containedFsw.SynchronizingObject.InvokeRequired)
                    _containedFsw.SynchronizingObject.BeginInvoke(handler, new object[] { this, e });
                else
                    handler(this, e);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
    }
    private void InvokeHandler(RenamedEventHandler eventHandler, RenamedEventArgs e)
    {
        if (eventHandler == null) return;

        foreach (RenamedEventHandler handler in eventHandler.GetInvocationList())
        {
            try
            {
                if (_containedFsw.SynchronizingObject != null && _containedFsw.SynchronizingObject.InvokeRequired)
                    _containedFsw.SynchronizingObject.BeginInvoke(handler, new object[] { this, e });
                else
                    handler(this, e);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
    }
    private void InvokeHandler(ErrorEventHandler eventHandler, ErrorEventArgs e)
    {
        if (eventHandler == null) return;

        foreach (ErrorEventHandler handler in eventHandler.GetInvocationList())
        {
            try
            {
                if (_containedFsw.SynchronizingObject != null && _containedFsw.SynchronizingObject.InvokeRequired)
                    _containedFsw.SynchronizingObject.BeginInvoke(handler, new object[] { this, e });
                else
                    handler(this, e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("An exception was thrown by a BufferingFileSystemWatcher Error handler: {0}", ex);
            }
        }
    }

    private void ReportError(Exception exception)
    {
        InvokeHandler(_onErrorHandler, new ErrorEventArgs(exception));
    }
    #endregion

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_lifecycleLock)
            {
                _containedFsw.EnableRaisingEvents = false;
                var pump = _activePump;
                _activePump = null;
                pump?.Cancel();
            }
            _containedFsw?.Dispose();
        }
        base.Dispose(disposing);
    }

    private sealed class EventPump : IDisposable
    {
        private readonly object _syncRoot = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private bool _disposed;

        public EventPump(int capacity)
        {
            Events = new BlockingCollection<FileSystemEventArgs>(capacity);
            Capacity = capacity;
        }

        public BlockingCollection<FileSystemEventArgs> Events { get; }
        public int Capacity { get; }
        public CancellationToken Token => _cancellationTokenSource.Token;

        public bool TryAdd(FileSystemEventArgs e)
        {
            lock (_syncRoot)
            {
                return _disposed || _cancellationTokenSource.IsCancellationRequested || Events.TryAdd(e);
            }
        }

        public void Cancel()
        {
            lock (_syncRoot)
            {
                if (!_disposed)
                    _cancellationTokenSource.Cancel();
            }
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (_disposed) return;
                _disposed = true;
                Events.Dispose();
                _cancellationTokenSource.Dispose();
            }
        }
    }
}
