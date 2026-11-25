using CadmanSDI.Entities;
using CadmanSDI.Host.Features.RFMR;
using CadmanSDI.Host.Helpers;
using CadmanSDI.Result;
using MetalHeaven.Agent.Shared.External.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CadmanSDI.Host.Features.ProcessResult;

public class CadmanSdiResultHandler : ICadmanSdiResult
{
    private readonly IRequestManufacturabilityResponse _response;
    private readonly ICadmanSdiResultMapper _resultMapper;
    private readonly ILogger<CadmanSdiResultHandler> _logger;
    private readonly CadmanSdiIntegrationSettings _integrationSettings;

    public CadmanSdiResultHandler(
        IRequestManufacturabilityResponse response,
        ICadmanSdiResultMapper resultMapper,
        ILogger<CadmanSdiResultHandler> logger,
        IOptions<CadmanSdiIntegrationSettings> integrationSettings
    )
    {
        _logger = logger;
        _response = response;
        _resultMapper = resultMapper;
        _integrationSettings = integrationSettings.Value;
    }

    public async Task ProcessResult(string resultFilePathName)
    {
        CadmanSdiResultData? cadmanSdiResult = null;
        try
        {
            cadmanSdiResult = _resultMapper.FromResultFile(resultFilePathName);

            var result = await _response.CreateFromResult(cadmanSdiResult, resultFilePathName);

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

    private async Task SaveResultToAgentInputFolder(RequestManufacturabilityCheckOfPartTypeMessageResponse result)
    {
        var agentUploadFolder = _integrationSettings.IntegrationInputFolder;

        // Save the result file in the Agent/CADMAN-SDI/Input folder
        var fileName = $"{result.PartTypeId.ToString()}.json";
        var responseFile = Path.Combine(agentUploadFolder, fileName);
        await File.WriteAllTextAsync(responseFile, JsonConvert.SerializeObject(result));
        _logger.LogInformation("Response for Rhodium24 saved in: {ResponseFile}", responseFile);
    }

    private void ReportException(Exception ex, CadmanSdiResultData? cadmanSdiResult = null)
    {
        if (cadmanSdiResult == null)
        {
            // result is empty and we don't have the project id/part id
            return;
        }

        var foundProjectId = Guid.TryParse(cadmanSdiResult.ProjectId, out var projectId);
        var foundPartId = Guid.TryParse(cadmanSdiResult.PartTypeId, out var partTypeId);

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
