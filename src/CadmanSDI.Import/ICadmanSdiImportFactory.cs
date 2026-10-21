using CadmanSDI.Entities.Models;
using MetalHeaven.Agent.Shared.External.Messages;

namespace CadmanSDI.Import;

public interface ICadmanSdiImportFactory
{
    CadManSdiImportV8_7 FromRequest(RequestManufacturabilityCheckOfPartTypeMessage request, string importFileName);
}
