using CadmanSDI.Entities;
using Integration.Common.FileWatcher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CadmanSDI.Host.Features.OutputFile;

/// <summary>
/// Service that watches on the output directory of the agent for *.json files
/// Processes JSON files created in the agent output directory
/// </summary>
public class OutputFileWatcherService : FileWatcherService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutputFileWatcherService> _logger;

    public OutputFileWatcherService(
        IServiceScopeFactory scopeFactory,
        IOptions<CadmanSdiIntegrationSettings> integrationSettings,
        ILogger<OutputFileWatcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        // add file watcher to the agent output directory

        var integrationOutputFolder = integrationSettings.Value.IntegrationOutputFolder;
        ProcessExistingFiles(integrationOutputFolder, ".json");
        AddFileWatcher(integrationOutputFolder, "*.json");
        _logger.LogInformation("File watch added on: '{Directory}' with filter: *.json", integrationOutputFolder);
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
        var handler = scope.ServiceProvider.GetRequiredService<OutputFileCreatedHandler>();
        handler.Handle(filePath, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    protected override void OnAllChanges(object sender, FileSystemEventArgs e)
    {
        try
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    ProcessFile(e.FullPath);
                    break;
                case WatcherChangeTypes.Deleted:
                case WatcherChangeTypes.Changed:
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
