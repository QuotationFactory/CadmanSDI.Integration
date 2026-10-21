namespace CadmanSDI.Host.Features.OutputFile;

public interface IRequestForManufacturability
{
    Task Import(string jsonFilePath);
}
