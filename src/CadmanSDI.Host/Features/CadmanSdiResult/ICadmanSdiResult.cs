namespace CadmanSDI.Host.Features.CadmanSdiResult;

public interface ICadmanSdiResult
{
    Task ProcessResult(string resultFilePathName);
}