using CadmanSDI.Entities.Models.V87;
using MetalHeaven.Agent.Shared.External.Messages;

namespace CadmanSDI.Export;

public interface ICadmanSdiRequestManufacturabilityResponseFactory
{
    Task<RequestManufacturabilityCheckOfPartTypeMessageResponse> CreateFromResult(CadManSdiExportV87 result, string resultFilePathName);
}

