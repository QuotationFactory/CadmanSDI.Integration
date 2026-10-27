using CadmanSDI.Entities.Models;

namespace CadmanSDI.Host.Features.CadmanSdiResult;

public interface ICadmanSdiResultMapper
{
    CadmanSdiResultData FromResultFile(string resultFilePathName);
}
