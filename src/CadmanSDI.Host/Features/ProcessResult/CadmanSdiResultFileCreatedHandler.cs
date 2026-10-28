using CadmanSDI.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CadmanSDI.Host.Features.ProcessResult;

public class CadmanSdiResultFileCreatedHandler
{
    private readonly CadmanSdiIntegrationSettings _settings;
    private readonly ILogger<CadmanSdiResultFileCreatedHandler> _logger;
    private readonly ICadmanSdiResult _cadManSdiResult;

    public CadmanSdiResultFileCreatedHandler(
        ILogger<CadmanSdiResultFileCreatedHandler> logger,
        ICadmanSdiResult cadManSdiResult,
        IOptions<CadmanSdiIntegrationSettings> settings)
    {
        _settings = settings.Value;
        _logger = logger;
        _cadManSdiResult = cadManSdiResult;
    }

    public async Task HandleAsync(string filePath, CancellationToken cancellationToken)
    {
        _logger.LogInformation("File created: {FilePath}", filePath);

        // default file creation timeout
        await Task.Delay(500, cancellationToken);

        _logger.LogInformation("Start processing request file: {FilePath}", filePath);
        await _cadManSdiResult.ProcessResult(filePath);

        var directory = Path.GetDirectoryName(filePath);
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
