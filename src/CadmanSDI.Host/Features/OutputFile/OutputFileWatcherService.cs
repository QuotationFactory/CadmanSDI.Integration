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
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutputFileWatcherService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public OutputFileWatcherService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<CadmanSdiIntegrationSettings> integrationSettings,
        ILogger<OutputFileWatcherService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        // add file watcher to the agent output directory

        var integrationOutputFolder = integrationSettings.Value.IntegrationOutputFolder;
        AddFileWatcher(integrationOutputFolder, "*.json");
        _logger.LogInformation("File watch added on: '{Directory}' with filter: *.json", integrationOutputFolder);
    }
#pragma warning disable VSTHRD100
    protected override async void OnAllChanges(object sender, FileSystemEventArgs e)
#pragma warning restore VSTHRD100
    {
        try
        {
            await _semaphore.WaitAsync();
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    await HandleCreatedFileAsync(e.FullPath, CancellationToken.None);
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
        finally
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


    private async Task HandleCreatedFileAsync(string filePath, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OutputFileCreatedHandler>();
        await handler.HandleAsync(filePath, cancellationToken);
    }
}
