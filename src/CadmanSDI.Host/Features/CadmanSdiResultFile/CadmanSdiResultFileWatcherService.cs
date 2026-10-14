using CadmanSDI.Entities;
using Integration.Common.FileWatcher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CadmanSDI.Host.Features.CadmanSdiResultFile;

/// <summary>
/// Service that watches on the output directory of the agent for *.json files
/// Processes XML files created in the Cadman-SDI output directory
/// </summary>
public class CadmanSdiResultFileWatcherService : FileWatcherService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CadmanSdiResultFileWatcherService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public CadmanSdiResultFileWatcherService(
        IOptions<CadmanSdiIntegrationSettings> settings,
        IServiceScopeFactory scopeFactory,
        ILogger<CadmanSdiResultFileWatcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        // add file watcher to the Cadman-SDI output directory
        var cadmanSdiResultsFolder = settings.Value.CadmanSdiResultsFolder;
        if (!Directory.Exists(cadmanSdiResultsFolder))
        {
            Directory.CreateDirectory(cadmanSdiResultsFolder);
        }
        AddFileWatcher(cadmanSdiResultsFolder, "*.xml", true);
        _logger.LogInformation("File watch added on: '{Directory}' with filter: *.xml", cadmanSdiResultsFolder);
    }

    private async Task HandleCreatedFileAsync(string filePath, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CadmanSdiResultFileCreatedHandler>();
        await handler.HandleAsync(filePath, cancellationToken);
    }

#pragma warning disable VSTHRD100
    protected override async void OnAllChanges(object sender, FileSystemEventArgs e)
#pragma warning restore VSTHRD100
    {
        try
        {   await _semaphore.WaitAsync();
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    await HandleCreatedFileAsync(e.FullPath, CancellationToken.None);
                    break;
                case WatcherChangeTypes.Deleted:
                case WatcherChangeTypes.Renamed:
                case WatcherChangeTypes.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e.ChangeType));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing {Event} for file {FilePath}", e.ChangeType, e.FullPath);
        }finally
        {
            _semaphore.Release();
        }
    }
#pragma warning disable VSTHRD100
    protected override async void OnExistingFile(object sender, FileSystemEventArgs e)
#pragma warning restore VSTHRD100
    {
        try
        {
            await _semaphore.WaitAsync();
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Deleted:
                case WatcherChangeTypes.Changed:
                case WatcherChangeTypes.Renamed:
                    break;
                case WatcherChangeTypes.All:
                    await HandleCreatedFileAsync(e.FullPath, CancellationToken.None);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing {Event} for file {FilePath}", e.ChangeType, e.FullPath);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
