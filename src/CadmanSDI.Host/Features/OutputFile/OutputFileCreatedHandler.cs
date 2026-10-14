using CadmanSDI.Host.Features.CadmanSdiImport;
using Microsoft.Extensions.Logging;

namespace CadmanSDI.Host.Features.OutputFile;

public class OutputFileCreatedHandler
{
    private readonly ILogger<OutputFileCreatedHandler> _logger;
    private readonly ICadmanSdiImport _cadmanSdiImport;

    public OutputFileCreatedHandler(
        ILogger<OutputFileCreatedHandler> logger,
        ICadmanSdiImport cadmanSdiImport
    )
    {
        _logger = logger;
        _cadmanSdiImport = cadmanSdiImport;
    }

    public async Task Handle(string filePath, CancellationToken cancellationToken)
    {
        _logger.LogInformation("File created: {FilePath}", filePath);

        // default file creation timeout
        await Task.Delay(500, cancellationToken);

        // define file paths
        var jsonFilePath = filePath;

        _logger.LogInformation("Start processing request file: {JsonFilePath}", jsonFilePath);
        await _cadmanSdiImport.Import(jsonFilePath);
    }
}
