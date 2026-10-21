using CadmanSDI.Entities;
using CadmanSDI.Entities.Models;
using CadmanSDI.Entities.Repositories;
using MetalHeaven.Agent.Shared.External.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CadmanSDI.Import;

public class CadmanSdiImportFactory : ICadmanSdiImportFactory
{
    private readonly ICadmanSdiImportMapper _mapper;
    private readonly IMaterialMappingRepository _materialMappingRepository;
    private readonly CadmanSdiIntegrationSettings _settings;
    private readonly ILogger<CadmanSdiImportFactory> _logger;

    public CadmanSdiImportFactory(
        ICadmanSdiImportMapper mapper,
        IMaterialMappingRepository materialMappingRepository,
        IOptions<CadmanSdiIntegrationSettings> settings,
        ILogger<CadmanSdiImportFactory> logger)
    {
        _mapper = mapper;
        _materialMappingRepository = materialMappingRepository;
        _settings = settings.Value;
        _logger = logger;
    }

    ICadmanSdiImport ICadmanSdiImportFactory.FromRequest(RequestManufacturabilityCheckOfPartTypeMessage request, string importFileName)
    {
        string partTypeName;
        if (_settings.SaveAsUniqueInclPartName)
        {
            var partTypeId = string.IsNullOrWhiteSpace(request.PartType.SellingPartyArticleNumber)
                ? request.PartType.Id.ToString()
                : request.PartType.SellingPartyArticleNumber;
            partTypeName = ToFileName($"{partTypeId}_{request.PartType.Name}");
        }
        else
        {
            partTypeName = _settings.SaveAsPartName
                ? ToFileName(request.PartType.Name)
                : request.PartType.Id.ToString();
        }

        var materialId = GetMaterialCode(request) ?? string.Empty;
        return _mapper.FromRequest(request, importFileName, partTypeName, materialId, _settings.CadmanSdiResultsFolder, _settings.CadmanSdiMacroName);
    }

    private string ToFileName(string? description)
    {
        var filename = string.IsNullOrWhiteSpace(description) ? "empty" : description.Trim();
        filename = string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        return filename;
    }

    private string? GetMaterialCode(RequestManufacturabilityCheckOfPartTypeMessage request)
    {
        var useMaterialMappingWithKeywords = _settings.MaterialMappingWithKeywords;

        if (useMaterialMappingWithKeywords)
        {
            var rhodiumMaterialKeywords = request.PartType.Material.SelectableArticles.SelectedArticle?.Tokens.ToArray() ?? [];
            var machineId = _materialMappingRepository.GetMaterialIdFromKeywords(rhodiumMaterialKeywords);
            if (string.IsNullOrWhiteSpace(machineId))
            {
                // Material not found in the repository, log error
                _logger.LogError(
                    "Material mapping not found in data/MaterialMapping.xlsx for Rhodium keywords: {RhodiumKeywords} ",
                    string.Join(", ", rhodiumMaterialKeywords));
            }
            return machineId;
        }
        else
        {
            var rhodiumMaterialId = request.PartType.Material.SelectableArticles.SelectedArticle?.Id ?? string.Empty;
            var machineId = _materialMappingRepository.GetMaterialIdFromArticle(rhodiumMaterialId);
            if (string.IsNullOrWhiteSpace(machineId))
            {
                // Material not found in the repository, log error
                _logger.LogError(
                    "Material mapping not found in data/MaterialMapping.xlsx for Rhodium material-id: {RhodiumMaterialId} ",
                    rhodiumMaterialId);
            }
            return machineId;
        }
    }
}
