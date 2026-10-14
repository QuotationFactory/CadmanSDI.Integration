namespace CadmanSDI.Host.Features.CadmanSdiImport;

public interface ICadmanSdiImport
{
    Task Import(string jsonFilePath);
}
