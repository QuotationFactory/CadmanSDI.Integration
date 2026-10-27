using System.Text;
using System.Xml.Serialization;
using CadmanSDI.Entities.Models.V87;

namespace CadmanSDI.Result;

public class CadmanSdiResultMapperV87 : ICadmanSdiResultMapper
{
    public CadmanSdiResultData FromResultFile(string resultFilePathName)
    {
        var result = Deserialize<CadManSdiExportV87>(resultFilePathName);
        var model = result.PARTTYPELIST?.PARTSHEETMETALTYPE?.PARTSHEETMETALMODELLIST?.PARTSHEETMETALMODEL;
        var job = result.JOBOSMCONVERTLIST?.JOBOSMCONVERT;
        var bendingExecution = result.EXECUTIONPLANPROPOSALLIST?.EXECUTIONPLANPROPOSAL
            ?.PATHLIST?.PATH?.STEPLIST?.STEP?.FirstOrDefault(step => step.Kind == "BENDINGEXEC");
        var bendingJobType = bendingExecution?.JOBTYPELIST?.JOBTYPE?.FirstOrDefault();
        var preparationJob = result.JOBPARTPREPARATIONLIST?.JOBPARTPREPARATION?.FirstOrDefault();

        return new CadmanSdiResultData(
            Prefer(model?.Comment, job?.OrderID),
            Prefer(model?.Description, job?.OSMCONVERTRESULT?.PartTypeID),
            job?.JobStatus,
            job?.JobStatusReason,
            preparationJob?.JobStatus,
            preparationJob?.JobStatusReason,
            bendingExecution?.Valid,
            bendingJobType?.SUMMARY?.Valid,
            bendingJobType?.SUMMARY?.Reason);
    }

    private static string? Prefer(string? value, string? fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static T Deserialize<T>(string resultFilePathName)
    {
        ArgumentNullException.ThrowIfNull(resultFilePathName);
        if (!File.Exists(resultFilePathName))
            throw new ApplicationException($"Result file does not exist. {resultFilePathName}");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var serializer = new XmlSerializer(typeof(T));
        using var reader = File.OpenRead(resultFilePathName);
        return (T)serializer.Deserialize(reader)! ??
               throw new ApplicationException("Deserializing CADMAN-SDI result failed");
    }
}
