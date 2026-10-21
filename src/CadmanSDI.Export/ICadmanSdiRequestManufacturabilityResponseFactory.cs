using CadmanSDI.Entities.Models.V8_7;
using MetalHeaven.Agent.Shared.External.Messages;

namespace CadmanSDI.Export;

public interface ICadmanSdiRequestManufacturabilityResponseFactory
{
    Task<RequestManufacturabilityCheckOfPartTypeMessageResponse> CreateFromResult(CadManSdiExportV8_7 result, string resultFilePathName);
}

