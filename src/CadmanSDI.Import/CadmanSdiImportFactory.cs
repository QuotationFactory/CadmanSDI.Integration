using System.Globalization;
using System.Text.RegularExpressions;
using CadmanSDI.Entities;
using CadmanSDI.Entities.Models;
using CadmanSDI.Entities.Repositories;
using MetalHeaven.Agent.Shared.External.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnitsNet.Units;

namespace CadmanSDI.Import;

public partial class CadmanSdiImportFactory : ICadmanSdiImportFactory
{
    private const string NumberOnly = "[^0-9.]";

    private static readonly Dictionary<LengthUnit, string> s_lengthUnits = new()
    {
        { LengthUnit.Millimeter, "MM" }
    };

    private readonly IMaterialMappingRepository _materialMappingRepository;
    private readonly CadmanSdiIntegrationSettings _settings;
    private readonly ILogger<CadmanSdiImportFactory> _logger;

    public CadmanSdiImportFactory(
        IMaterialMappingRepository materialMappingRepository,
        IOptions<CadmanSdiIntegrationSettings> settings,
        ILogger<CadmanSdiImportFactory> logger)
    {
        _materialMappingRepository = materialMappingRepository;
        _settings = settings.Value;
        _logger = logger;
    }

    CadManSdiImportV8_7 ICadmanSdiImportFactory.FromRequest(RequestManufacturabilityCheckOfPartTypeMessage request, string importFileName)
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
        return FromRequest(request, importFileName, partTypeName, materialId, _settings.CadmanSdiResultsFolder, _settings.CadmanSdiMacroName);
    }

    private string ToFileName(string? description)
    {
        var filename = string.IsNullOrWhiteSpace(description) ? "empty" : description.Trim();
        filename = string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        return filename;
    }

    public static CadManSdiImportV8_7 FromRequest(
        RequestManufacturabilityCheckOfPartTypeMessage request,
        string importFileName,
        string partTypeName,
        string materialId,
        string cadmanSdiResultsFolder,
        string macroName
        )
    {
        if (string.IsNullOrWhiteSpace(importFileName))
            throw new ArgumentNullException(nameof(importFileName));

        if (string.IsNullOrWhiteSpace(partTypeName))
            partTypeName = request.PartType.Name;

        if (string.IsNullOrWhiteSpace(materialId))
        {
            if (request.PartType.Material.SelectableArticles.SelectedArticle != null)
            {
                materialId = request.PartType.Material.SelectableArticles.SelectedArticle.Id;
            }
        }

        var response = new CadManSdiImportV8_7
        {
            PartTypeImport = new PartTypeImport
            {
                PartTypeList = new PartTypeList
                {
                    PartTypes = new List<PartType>
                    {
                        new()
                        {
                            Id = partTypeName,
                            Comment = request.ProjectId.ToString(),
                            Description = request.PartType.Id.ToString(),
                            MaterialId = materialId,
                            Importfilename = importFileName
                        }
                    }
                }
            },
            Batch = new Batch
            {
                Commands = new List<Command>
                {
                    new()
                    {
                        CommandString = string.IsNullOrWhiteSpace(macroName) ? "HANDLEFILE_BATCH" : macroName,
                        Parameters =
                        [
                            new Parameter
                            {
                                Id = "PARTTYPEID",
                                Value = partTypeName
                            },
                            new Parameter
                            {
                                Id = "OUTPUTPATH",
                                Value = cadmanSdiResultsFolder
                            }
                        ]
                    }
                }
            }
        };

        if (request.PartType.Material.SelectableArticles.SelectedArticle == null)
        {
            return response;
        }

        var selectedArticle = request.PartType.Material.SelectableArticles.SelectedArticle;
        var thicknessHasValue = selectedArticle.Dimensions.Thickness is not null;
        var thickness = thicknessHasValue ? selectedArticle.Dimensions.Thickness!.Value : default;

        response.PartTypeImport.PartTypeList.PartTypes.First().Thickness = MyRegex().Replace(thickness.Value.ToString(CultureInfo.InvariantCulture), "");
        response.PartTypeImport.PartTypeList.PartTypes.First().Unit = s_lengthUnits[thickness.Unit];

        return response;
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

    [GeneratedRegex(NumberOnly)]
    private static partial Regex MyRegex();
}
