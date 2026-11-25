namespace CadmanSDI.Result;

public interface ICadmanSdiResultMapper
{
    CadmanSdiResultData FromResultFile(string resultFilePathName);
}
