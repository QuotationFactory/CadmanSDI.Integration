using CadmanSDI.Entities.Models;
using MetalHeaven.Agent.Shared.External.Messages;

namespace CadmanSDI.Import;

public interface ICadmanSdiImportFactory
{
    ICadmanSdiImport FromRequest(RequestManufacturabilityCheckOfPartTypeMessage request, string importFileName);
}
