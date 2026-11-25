namespace CadmanSDI.Host.Features.RFM;

public interface IRequestForManufacturability
{
    Task Import(string jsonFilePath);
}
