using CadmanSDI.Entities;
using CadmanSDI.Entities.Models.V8_7;
using MetalHeaven.Agent.Shared.External.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CadmanSDI.Export;

public class CadmanSdiRequestManufacturabilityResponseFactory : ICadmanSdiRequestManufacturabilityResponseFactory
{
    private readonly CadmanSdiIntegrationSettings _settings;
    private readonly ILogger<CadmanSdiRequestManufacturabilityResponseFactory> _logger;

    public CadmanSdiRequestManufacturabilityResponseFactory(
        IOptions<CadmanSdiIntegrationSettings> settings,
        ILogger<CadmanSdiRequestManufacturabilityResponseFactory> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<RequestManufacturabilityCheckOfPartTypeMessageResponse> CreateFromResult(CadManSdiExportV8_7 result, string resultFilePathName)
    {
        ArgumentNullException.ThrowIfNull(result);

        var response = new RequestManufacturabilityCheckOfPartTypeMessageResponse();

        if (Guid.TryParse(result.PARTTYPELIST.PARTSHEETMETALTYPE.PARTSHEETMETALMODELLIST.PARTSHEETMETALMODEL.Description, out var foundPartTypeGuid))
        {
            response.PartTypeId = foundPartTypeGuid;
        }
        else
        {
            throw new ArgumentNullException(
                nameof(result.PARTTYPELIST.PARTSHEETMETALTYPE.PARTSHEETMETALMODELLIST.PARTSHEETMETALMODEL.Description),
                "Could not retrieve partType id from part.");
        }

        if (Guid.TryParse(result.PARTTYPELIST.PARTSHEETMETALTYPE.PARTSHEETMETALMODELLIST.PARTSHEETMETALMODEL.Comment,
                out var foundProjectIdGuid))
        {
            response.ProjectId = foundProjectIdGuid;
        }
        else
        {
            throw new ArgumentNullException(
                nameof(result.PARTTYPELIST.PARTSHEETMETALTYPE.PARTSHEETMETALMODELLIST.PARTSHEETMETALMODEL.Comment),
                "Could not retrieve project id from part comment.");
        }

        // response.EventLogs = GetLogs(result, response.ProjectId, response.PartTypeId);
        response.EventLogs = Array.Empty<EventLog>();

        // Assume manufacturable if there are no Failed tests
        // response.IsManufacturable = response.EventLogs.All(log => log.Level != EventLogLevel.Error);
        response.IsManufacturable = TryGetIsManufacturable(result, response, out var logs);
        if (logs.Count != 0)
        {
            response.EventLogs = logs.ToArray();
        }

        response.Documents = GetResponseDocuments(resultFilePathName);

        return Task.FromResult(response);
    }

    private bool TryGetIsManufacturable(CadManSdiExportV8_7 result, RequestManufacturabilityCheckOfPartTypeMessageResponse response, out List<EventLog> eventLogs)
    {
        eventLogs = new List<EventLog>();
        if (result.JOBOSMCONVERTLIST.JOBOSMCONVERT.JobStatus != "Finished")
        {
            eventLogs.Add(new EventLog()
            {
                ProjectId = response.ProjectId,
                PartTypeId = response.PartTypeId,
                Message = result.JOBOSMCONVERTLIST.JOBOSMCONVERT.JobStatusReason,
                Level = EventLogLevel.Error
            });
            return false;
        }

        var jobPreparation = result.JOBPARTPREPARATIONLIST.JOBPARTPREPARATION.FirstOrDefault();
        if (jobPreparation is not null && jobPreparation.JobStatus != "Finished")
        {
            eventLogs.Add(new EventLog()
            {
                ProjectId = response.ProjectId,
                PartTypeId = response.PartTypeId,
                Message = jobPreparation.JobStatusReason,
                Level = EventLogLevel.Error
            });
            return false;
        }

        var partStepBendingExecution = result.EXECUTIONPLANPROPOSALLIST.EXECUTIONPLANPROPOSAL.PATHLIST.PATH.STEPLIST.STEP.FirstOrDefault(x=>x.Kind == "BENDINGEXEC");
        if (partStepBendingExecution is null)
        {
            eventLogs.Add(new EventLog()
            {
                ProjectId = response.ProjectId,
                PartTypeId = response.PartTypeId,
                Message = "BENDINGEXEC is missing",
                Level = EventLogLevel.Error
            });
            return false;
        }

        if (partStepBendingExecution.Valid != "1")
        {
            var jobType = partStepBendingExecution.JOBTYPELIST.JOBTYPE.FirstOrDefault();
            if (jobType is not null && jobType.SUMMARY.Valid != "1")
            {
                eventLogs.Add(new EventLog()
                {
                    ProjectId = response.ProjectId,
                    PartTypeId = response.PartTypeId,
                    Message = jobType.SUMMARY.Reason,
                    Level = EventLogLevel.Error
                });
                return false;
            }
        }

        return true;
    }

    private IEnumerable<ResponseDocument> GetResponseDocuments(string resultFilePathName)
    {
        List<ResponseDocument> documents = new();

        // Add DXF file
        if (_settings.UploadDxfFile)
        {
            var filePathName =
                Path.GetDirectoryName(resultFilePathName) + '\\' +
                Path.GetFileNameWithoutExtension(resultFilePathName) + ".dxf";
            var dxfDoc = GetResponseDocument(filePathName);

            if (dxfDoc != null)
            {
                documents.Add(dxfDoc);
            }
            else
            {
                _logger.LogWarning("Could not get DXF file to add");
            }
        }

        // Add SDI prj file
        if (_settings.UploadSdiPrj)
        {
            var filePathName =
                Path.GetDirectoryName(resultFilePathName) + '\\' +
                Path.GetFileNameWithoutExtension(resultFilePathName) + ".SDIprj";
            var sdiPrjDoc = GetResponseDocument(filePathName);

            if (sdiPrjDoc != null)
            {
                documents.Add(sdiPrjDoc);
            }
            else
            {
                _logger.LogWarning("Could not get SDIprj file to add");
            }
        }

        if (!_settings.UploadResultXmlFile)
        {
            return documents;
        }

        // Add the result XML file
        var resultDoc = GetResponseDocument(resultFilePathName);
        if (resultDoc != null)
        {
            documents.Add(resultDoc);
        }
        else
        {
            _logger.LogWarning("Could not get result XML file to add");
        }

        return documents;
    }

    private ResponseDocument? GetResponseDocument(string filePathName)
    {
        if (string.IsNullOrWhiteSpace(filePathName))
        {
            return null;
        }

        if (!File.Exists(filePathName))
        {
            _logger.LogWarning("Document not found {FilePathName}", filePathName);
            return null;
        }

        var fileAsBase64 = GetFileAsBase64(filePathName);
        var fileName = Path.GetFileName(filePathName);

        var doc = new ResponseDocument
        {
            DocumentName = fileName,
            DocumentContent = fileAsBase64
        };

        return doc;
    }

    private static string GetFileAsBase64(string filePathName)
    {
        var bytes = File.ReadAllBytes(filePathName);
        var base64String = Convert.ToBase64String(bytes);
        return base64String;
    }
}
