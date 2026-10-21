using CadmanSDI.Entities.Models;
using CadmanSDI.Entities.Models.V8_7;
using MetalHeaven.Agent.Shared.External.Messages;

namespace CadmanSDI.Import;

public interface ICadmanSdiImportFactory
{
    //CadManSdiImportV8_7 FromRequest(RequestManufacturabilityCheckOfPartTypeMessage request, string importFileName);
    ICadmanSdiImport FromRequest(RequestManufacturabilityCheckOfPartTypeMessage request, string importFileName);
}
