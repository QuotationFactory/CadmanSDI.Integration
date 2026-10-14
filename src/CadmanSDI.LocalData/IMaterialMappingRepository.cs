namespace CadmanSDI.Entities.Repositories;

public interface IMaterialMappingRepository
{
    string GetMaterialIdFromKeywords(string[] keywords);

    string GetMaterialIdFromArticle(string rhodiumMaterialId);

}
