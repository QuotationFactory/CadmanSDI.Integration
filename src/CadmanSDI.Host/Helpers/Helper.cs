using MetalHeaven.Agent.Shared.External.Messages;
using Newtonsoft.Json;
using Versioned.ExternalDataContracts.Contracts.Resource;
using Versioned.ExternalDataContracts.Enums;

namespace CadmanSDI.Host.Helpers;

public static class Helper
{
    public static void SaveExceptionMessageToResponseInputFolder(Guid projectId, Guid partTypeId, string message, string inputFolder)
    {
        var response = new RequestManufacturabilityCheckOfPartTypeMessageResponse
        {
            ProjectId = projectId,
            PartTypeId = partTypeId,
            IsManufacturable = false
        };
        var logs = new List<EventLog>
        {
            new EventLog
            {
                DateTime = DateTime.UtcNow,
                Message = $"Error. {message}",
                Level = EventLogLevel.Error,
                ProjectId = projectId,
                PartTypeId = partTypeId
            }
        };
        response.EventLogs = logs;

        // Save the result file in the Agent/integration/Input folder
        var fileName = $"{partTypeId.ToString()}.json";
        var responseFile = Path.Combine(inputFolder, fileName);
        File.WriteAllTextAsync(responseFile, JsonConvert.SerializeObject(response));
    }
}
