using CadmanSDI.LocalData.Models;

namespace CadmanSDI.LocalData;

public interface IManufacturabilityRequestRepository
{
    Task<ManufacturabilityRequestInfoModel?> GetRequestInfoFromRequestId(string requestId);
}
