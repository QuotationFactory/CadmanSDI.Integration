using System.Net;
using System.Xml.Serialization;
using CadmanSDI.Entities;
using CadmanSDI.Host.Helpers;
using CadmanSDI.Import;
using Integration.Common.Serialization;
using MetalHeaven.Agent.Shared.External.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Versioned.ExternalDataContracts;

namespace CadmanSDI.Host.Features.OutputFile;

public class RequestForManufacturability : IRequestForManufacturability
{
    private readonly CadmanSdiIntegrationSettings _settings;
    private readonly ICadmanSdiImportFactory _inputFactory;
    private readonly ILogger<RequestForManufacturability> _logger;

    public RequestForManufacturability(
        ICadmanSdiImportFactory inputFactory,
        ILogger<RequestForManufacturability> logger,
        IOptions<CadmanSdiIntegrationSettings> settings)
    {
        _inputFactory = inputFactory;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task Import(string jsonFilePath)
    {
        RequestManufacturabilityCheckOfPartTypeMessage? request = null;
        try
        {
            request = await GetRequestManufacturabilityCheckOfPartTypeMessage(jsonFilePath);
            if (request == null)
            {
                throw new ApplicationException($"Failed to deserialize request from file: '{jsonFilePath}'.");
            }

            // The file name is a GUID, we use it as the request ID, the files will be all named with the same name
            // we can use this later while reading the result, so that we know to which request a response belongs
            var requestId = Path.GetFileNameWithoutExtension(jsonFilePath);

            var stepFilePathName = await DownloadStepFileAndReturnPathAsync(request);

            // Create the CADMAN-SDI import file (BatchInput)
            var import = _inputFactory.FromRequest(request, stepFilePathName);

            SaveImportFileInWatchFolder(requestId, import);

            _logger.LogInformation("Finished import cadman-sdi: {Id}", request.ProjectId);
            _settings.MoveFileToProcessed(Constants.AgentIntegrationName, jsonFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while handling project files");
            _settings.MoveFileToError(Constants.AgentIntegrationName, jsonFilePath);
            if (request != null)
            {
                ReportException(ex, request);
            }
        }
    }

    private void SaveImportFileInWatchFolder(string requestId, Entities.Models.ICadmanSdiImport import)
    {
        // Save import file in cadman-sdi watch folder
        // Make the name the request-id, then we can use this as a reference when reading the file
        var fileNameOnly = requestId + ".xml";
        var fullFileName = Path.Combine(_settings.CadmanSdiWatchFolder, fileNameOnly);
        // save
        var xmlSerializer = new XmlSerializer(import.GetType());

        using var file = File.Create(fullFileName);
        xmlSerializer.Serialize(file, import);
        _logger.LogInformation("Saved batch input file: {FullFileName}", fullFileName);
    }

    private async Task<string> DownloadStepFileAndReturnPathAsync(RequestManufacturabilityCheckOfPartTypeMessage request)
    {
        var stepDownloadDirectory = _settings.CadmanSdiStepFolder;
        var stepFileUrl = request.StepFileUrl;
        // Step file name will have the same name as the part-id, with the extension .step
        var partId = request.PartType.Id.ToString();
        var stepFilePathName = Path.Combine(stepDownloadDirectory, $"{partId}.step");

        // Download the step file sync
        await DownloadFileAsync(stepFileUrl, stepFilePathName);
        _logger.LogInformation("Downloaded step file: {StepFilePathName}", stepFilePathName);
        return stepFilePathName;
    }

    private static async Task<RequestManufacturabilityCheckOfPartTypeMessage?> GetRequestManufacturabilityCheckOfPartTypeMessage(
        string jsonFilePath)
    {
        // check if json file exists
        if (!File.Exists(jsonFilePath))
        {
            throw new ApplicationException($"Result file does not exist. Path: '{jsonFilePath}'.");
        }

        // define json serializer settings
        var settings = new JsonSerializerSettings();
        settings.SetJsonSettings();
        settings.AddJsonConverters();
        settings.SerializationBinder = new CrossPlatformTypeBinder();

        // read all text from file that is created
        var json = await File.ReadAllTextAsync(jsonFilePath);

        // convert json to project object
        return JsonConvert.DeserializeObject<RequestManufacturabilityCheckOfPartTypeMessage>(json, settings);
    }

    private async Task DownloadFileAsync(Uri stepFileUrl, string filePathName)
    {
        // Download sync, because we want to ensure the file is downloaded before continuing
        await DownloadFileAsync(stepFileUrl.AbsoluteUri, filePathName);
    }

    private async Task DownloadFileAsync(string uri, string outputPath)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out _))
            throw new InvalidOperationException("URI is invalid.");

        var httpClient = new HttpClient();

        try
        {
            var fileBytes = await httpClient.GetByteArrayAsync(uri);
            await File.WriteAllBytesAsync(outputPath, fileBytes);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogError("Not authorized to download step file with URI: '{uri}'. " +
                                 "Continuing in development for testing purposes...", uri);
                throw new InvalidOperationException("Cannot continue without input file.", e);
            }
        }
    }

    private void ReportException(Exception ex, RequestManufacturabilityCheckOfPartTypeMessage request)
    {
        var projectId = request.ProjectId;
        var partTypeId = request.PartType.Id;
        var message = ex.Message;
        var agentUploadFolder = _settings.GetInputDirectory(integrationName: Constants.AgentIntegrationName);
        Helper.SaveExceptionMessageToResponseInputFolder(projectId, partTypeId, message, agentUploadFolder);
    }
}
