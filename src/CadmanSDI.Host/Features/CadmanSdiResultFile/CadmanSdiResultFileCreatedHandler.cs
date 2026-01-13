using CadmanSDI.Entities;
using CadmanSDI.Host.Features.CadmanSdiResult;
using CadmanSDI.Host.Features.OutputFile;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CadmanSDI.Host.Features.CadmanSdiResultFile;

public class CadmanSdiResultFileCreatedHandler : INotificationHandler<CadmanSdiResultFileCreated>
{
    private readonly CadmanSdiIntegrationSettings _settings;
    private readonly ILogger<CadmanSdiResultFileCreatedHandler> _logger;
    private readonly ICadmanSdiResult _cadmanSdiResult;

    public CadmanSdiResultFileCreatedHandler(
        ILogger<CadmanSdiResultFileCreatedHandler> logger,
        ICadmanSdiResult cadmanSdiResult,
        IOptions<CadmanSdiIntegrationSettings> settings)
    {
        _settings = settings.Value;
        _logger = logger;
        _cadmanSdiResult = cadmanSdiResult;
    }

    public async Task Handle(CadmanSdiResultFileCreated notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("File created: {FilePath}", notification.FilePath);

        // default file creation timeout
        await Task.Delay(500, cancellationToken);

        // define file paths
        var jsonFilePath = notification.FilePath;

        _logger.LogInformation("Start processing request file: {JsonFilePath}", jsonFilePath);
        await _cadmanSdiResult.ProcessResult(jsonFilePath);

        var directory = Path.GetDirectoryName(jsonFilePath);
        if (directory is null)
        {
            throw new Exception("Directory could not be created");
        }

        var lastDirectoryName = Path.GetFileName(directory);
        var files = Directory.EnumerateFiles(directory);

        foreach (var file in files)
        {
            file.MoveFileToDirectory($"{_settings.CadmanSdiResultsProcessedFolder}/{lastDirectoryName}");
        }

        Directory.Delete(directory);
    }
}
