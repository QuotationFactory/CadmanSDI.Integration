using CadmanSDI.Entities.Models;
using MetalHeaven.Agent.Shared.External.Messages;

namespace CadmanSDI.Import;

public interface ICadmanSdiImportMapper
{
    ICadmanSdiImport FromRequest(
        RequestManufacturabilityCheckOfPartTypeMessage request,
        string importFileName,
        string partTypeName,
        string materialId,
        string cadmanSdiResultsFolder,
        string macroName);
}
