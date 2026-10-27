namespace CadmanSDI.LocalData;

public interface IMaterialMappingRepository
{
    string? GetMaterialIdFromKeywords(string[] keywords);

    string? GetMaterialIdFromArticle(string rhodiumMaterialId);

}
