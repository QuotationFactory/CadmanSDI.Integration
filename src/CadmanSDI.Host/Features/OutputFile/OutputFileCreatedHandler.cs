using CadmanSDI.Host.Features.RFM;
using Microsoft.Extensions.Logging;

namespace CadmanSDI.Host.Features.OutputFile;

public class OutputFileCreatedHandler
{
    private readonly ILogger<OutputFileCreatedHandler> _logger;
    private readonly IRequestForManufacturability _requestForManufacturability;

    public OutputFileCreatedHandler(
        ILogger<OutputFileCreatedHandler> logger,
        IRequestForManufacturability requestForManufacturability
    )
    {
        _logger = logger;
        _requestForManufacturability = requestForManufacturability;
    }

    public async Task HandleAsync(string filePath, CancellationToken cancellationToken)
    {
        _logger.LogInformation("File created: {FilePath}", filePath);

        // default file creation timeout
        await Task.Delay(500, cancellationToken);

        _logger.LogInformation("Start processing request file: {FilePath}", filePath);
        await _requestForManufacturability.Import(filePath);
    }
}
