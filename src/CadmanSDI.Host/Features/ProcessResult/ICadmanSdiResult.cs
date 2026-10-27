namespace CadmanSDI.Host.Features.ProcessResult;

public interface ICadmanSdiResult
{
    Task ProcessResult(string resultFilePathName);
}
