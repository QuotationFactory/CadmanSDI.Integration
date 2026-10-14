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
        ProcessExistingFiles(cadmanSdiResultsFolder, ".xml");
        AddFileWatcher(cadmanSdiResultsFolder, "*.xml", true);
        _logger.LogInformation("File watch added on: '{Directory}' with filter: *.xml", cadmanSdiResultsFolder);
    }

    private void ProcessExistingFiles(string directory, string extensionfilter)
    {
        var existingFiles = Directory.EnumerateFiles(directory).Where(x => Path.GetExtension(x) == extensionfilter).ToArray();
        if (!existingFiles.Any())
        {
            return;
        }

        foreach (var file in existingFiles)
        {
            ProcessFile(file);
        }
    }

    private void ProcessFile(string filePath)
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CadmanSdiResultFileCreatedHandler>();
        handler.Handle(filePath, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    protected override void OnAllChanges(object sender, FileSystemEventArgs e)
    {
        try
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    ProcessFile(e.FullPath);
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
        }
    }
}
