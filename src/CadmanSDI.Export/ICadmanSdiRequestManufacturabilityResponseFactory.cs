using CadmanSDI.Result;
using MetalHeaven.Agent.Shared.External.Messages;

namespace CadmanSDI.Export;

public interface ICadmanSdiRequestManufacturabilityResponseFactory
{
    Task<RequestManufacturabilityCheckOfPartTypeMessageResponse> CreateFromResult(
        CadmanSdiResultData result,
        string resultFilePathName);
}
