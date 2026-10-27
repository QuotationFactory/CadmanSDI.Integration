using CadmanSDI.Result;
using MetalHeaven.Agent.Shared.External.Messages;

namespace CadmanSDI.Host.Features.RFMR;

public interface IRequestManufacturabilityResponse
{
    Task<RequestManufacturabilityCheckOfPartTypeMessageResponse> CreateFromResult(
        CadmanSdiResultData result,
        string resultFilePathName);
}
