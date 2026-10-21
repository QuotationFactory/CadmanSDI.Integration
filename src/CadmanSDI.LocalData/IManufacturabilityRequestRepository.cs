using CadmanSDI.LocalData.Models;

namespace CadmanSDI.Entities.Repositories;

public interface IManufacturabilityRequestRepository
{
    Task<ManufacturabilityRequestInfoModel?> GetRequestInfoFromRequestId(string requestId);
}
