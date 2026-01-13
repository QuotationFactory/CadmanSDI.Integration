using CadmanSDI.Host.Features.CadmanSdiImport;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CadmanSDI.Host.Features.OutputFile;

public class OutputFileCreatedHandler : INotificationHandler<OutputFileCreated>
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

    public async Task Handle(OutputFileCreated notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("File created: {FilePath}", notification.FilePath);

        // default file creation timeout
        await Task.Delay(500, cancellationToken);

        // define file paths
        var jsonFilePath = notification.FilePath;

        _logger.LogInformation("Start processing request file: {JsonFilePath}", jsonFilePath);
        await _cadmanSdiImport.Import(jsonFilePath);
    }
}
