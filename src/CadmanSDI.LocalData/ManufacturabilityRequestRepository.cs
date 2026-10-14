using CadmanSDI.Entities;
using CadmanSDI.Entities.Models;
using CadmanSDI.Entities.Repositories;
using CadmanSDI.LocalData.Models;
using MetalHeaven.Agent.Shared.External.Messages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Versioned.ExternalDataContracts;

namespace CadmanSDI.LocalData;

public class ManufacturabilityRequestRepository : IManufacturabilityRequestRepository
{
    private readonly CadmanSdiIntegrationSettings _settings;

    public ManufacturabilityRequestRepository(IOptions<CadmanSdiIntegrationSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<ManufacturabilityRequestInfoModel?> GetRequestInfoFromRequestId(string requestId)
    {
        // Find the original request in the LVD CADMAN-SDI\Output\Processed folder
        // Retrieve the project and part id from the request

        //var processedPath = Path.Combine(_settings.CadmanSdiResultsProcessedFolder, Entities.Constants.AgentIntegrationName, "Output", "Processed");
        var requestFilePathName = Path.Combine(_settings.CadmanSdiResultsProcessedFolder, $"{requestId}.json");
        if (!File.Exists(requestFilePathName))
        {
            return null;
        }

        var settings = new JsonSerializerSettings();
        settings.SetJsonSettings();
        settings.AddJsonConverters();

        // read all text from file that is created
        var json = await File.ReadAllTextAsync(requestFilePathName);

        // convert json to project object
        var request = JsonConvert.DeserializeObject<RequestManufacturabilityCheckOfPartTypeMessage>(json, settings);

        if (request == null)
        {
            return null;
        }

        var result = new ManufacturabilityRequestInfoModel
        {
            RequestId = Path.GetFileNameWithoutExtension(requestId),
            ProjectId = request.ProjectId.ToString(),
            PartId = request.PartType.Id.ToString()
        };

        return result;
    }
}
