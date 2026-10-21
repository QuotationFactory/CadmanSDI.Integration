using System.Text;
using System.Xml.Serialization;
using CadmanSDI.Entities;
using CadmanSDI.Entities.Models.V8_7;
using CadmanSDI.Export;
using CadmanSDI.Host.Features.OutputFile;
using CadmanSDI.Host.Helpers;
using MetalHeaven.Agent.Shared.External.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CadmanSDI.Host.Features.CadmanSdiResult;

public class CadmanSdiResult : ICadmanSdiResult
{
    private readonly ICadmanSdiRequestManufacturabilityResponseFactory _responseFactory;
    private readonly ILogger<CadmanSdiResult> _logger;
    private readonly CadmanSdiIntegrationSettings _integrationSettings;

    public CadmanSdiResult(
        ICadmanSdiRequestManufacturabilityResponseFactory responseFactory,
        ILogger<CadmanSdiResult> logger,
        IOptions<CadmanSdiIntegrationSettings> integrationSettings
    )
    {
        _logger = logger;
        _responseFactory = responseFactory;
        _integrationSettings = integrationSettings.Value;
    }

    public async Task ProcessResult(string resultFilePathName)
    {
        CadManSdiExportV8_7? cadmanSdiResult = null;
        try
        {
            cadmanSdiResult = GetCadmanSdiResult(resultFilePathName);

            var resultsPath = Path.GetDirectoryName(resultFilePathName) ??
                              throw new InvalidOperationException("Could not get results path");

            var result = await _responseFactory.CreateFromResult(cadmanSdiResult, resultFilePathName);

            await SaveResultToAgentInputFolder(result);
            // TODO: move files to processed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file: {ResultFilePathName}", resultFilePathName);

            // TODO: move file to cadmansdi-results/error folder
            ReportException(ex, cadmanSdiResult);
        }
    }

    private CadManSdiExportV8_7 GetCadmanSdiResult(string resultFilePathName)
    {
        CadManSdiExportV8_7 cadmanCadManSdiResult;
        if (resultFilePathName == null)
        {
            throw new ArgumentNullException(nameof(resultFilePathName));
        }

        // check if json file exists
        if (!File.Exists(resultFilePathName))
        {
            throw new ApplicationException($"Result file does not exists. {resultFilePathName}");
        }

        // Cadman-b produces on some machines XML files with encoding="windows-1252"
        // windows-1252 is not supported out of the box by .net core 5
        // To support other "exotic" encodings we need to include the nuget package:
        // https://www.nuget.org/packages/System.Text.Encoding.CodePages/
        // And register it with: Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        // This will allow reading of xml files with "exotic" encodings
        // More info: https://docs.microsoft.com/en-us/dotnet/api/system.text.codepagesencodingprovider?view=net-5.0
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Deserialize xml result file to poco classes
        XmlSerializer serializer = new XmlSerializer(typeof(CadManSdiExportV8_7));

        using (Stream reader = new FileStream(resultFilePathName, FileMode.Open))
        {
            cadmanCadManSdiResult = (CadManSdiExportV8_7)serializer.Deserialize(reader)! ??
                              throw new ApplicationException("Deserializing CADMAN-SDI result failed");;
        }

        return cadmanCadManSdiResult;
    }

    private async Task SaveResultToAgentInputFolder(RequestManufacturabilityCheckOfPartTypeMessageResponse result)
    {
        var agentUploadFolder = _integrationSettings.IntegrationInputFolder;

        // Save the result file in the Agent/CADMAN-B/Input folder
        var fileName = $"{result.PartTypeId.ToString()}.json";
        var responseFile = Path.Combine(agentUploadFolder, fileName);
        await File.WriteAllTextAsync(responseFile, JsonConvert.SerializeObject(result));
        _logger.LogInformation("Response for Rhodium24 saved in: {ResponseFile}", responseFile);
    }

    private void ReportException(Exception ex, CadManSdiExportV8_7? cadmanSdiResult = null)
    {
        if (cadmanSdiResult == null)
        {
            // result is empty and we don't have the project id/part id
            return;
        }

        var foundProjectId = Guid.TryParse(cadmanSdiResult.JOBOSMCONVERTLIST.JOBOSMCONVERT.OrderID, out var projectId);
        var foundPartId = Guid.TryParse(cadmanSdiResult.JOBOSMCONVERTLIST.JOBOSMCONVERT.OSMCONVERTRESULT.PartTypeID, out var partTypeId);

        if (!foundProjectId || !foundPartId)
        {
            // Cannot return response, because we miss the project/part id
            return;
        }

        var message = ex.Message;
        var agentUploadFolder =
            _integrationSettings.GetOrCreateAgentInputDirectory(integrationName: Entities.Constants.AgentIntegrationName, createIfNotExists: true);

        Helper.SaveExceptionMessageToResponseInputFolder(projectId, partTypeId, message, agentUploadFolder);
    }
}
