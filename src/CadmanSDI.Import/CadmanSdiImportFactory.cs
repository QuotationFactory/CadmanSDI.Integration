using System.Text.RegularExpressions;
using CadmanSDI.Entities;
using CadmanSDI.Entities.Models;
using CadmanSDI.Entities.Repositories;
using MetalHeaven.Agent.Shared.External.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnitsNet.Units;

namespace CadmanSDI.Import;

public class CadmanSdiImportFactory : ICadmanSdiImportFactory
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

    SdiImport ICadmanSdiImportFactory.FromRequest(RequestManufacturabilityCheckOfPartTypeMessage request, string importFileName)
    {
        string partTypeId;
        if (_settings.SaveAsUniqueInclPartName)
        {
            partTypeId = ToFileName($"{request.PartType.Id.ToString()}_{request.PartType.Name}");
        }
        else
        {
            partTypeId = _settings.SaveAsPartName
                ? ToFileName(request.PartType.Name)
                : request.PartType.Id.ToString();
        }

        var materialId = GetMaterialCode(request);
        return FromRequest(request, importFileName, partTypeId, materialId, _settings.CadmanSdiResultsFolder);
    }

    private string ToFileName(string? description)
    {
        var filename = string.IsNullOrWhiteSpace(description) ? "empty" : description.Trim();
        filename = string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        return filename;
    }

    public static SdiImport FromRequest(
        RequestManufacturabilityCheckOfPartTypeMessage request,
        string importFileName,
        string partTypeId,
        string materialId,
        string cadmanSdiResultsFolder
        )
    {
        if (string.IsNullOrWhiteSpace(importFileName))
            throw new ArgumentNullException(nameof(importFileName));

        if (string.IsNullOrWhiteSpace(partTypeId))
            partTypeId = request.PartType.Name;

        if (string.IsNullOrWhiteSpace(materialId))
            materialId = request.PartType.Material.SelectableArticles.SelectedArticle.Id;

        var response = new SdiImport
        {
            PartTypeImport = new PartTypeImport
            {
                PartTypeList = new PartTypeList
                {
                    PartTypes = new List<PartType>
                    {
                        new()
                        {
                            Id = partTypeId,
                            Comment = request.ProjectId.ToString(),
                            Description = request.PartType.Id.ToString(),
                            MaterialId = materialId,
                            Thickness =
                                Regex.Replace(
                                    request.PartType.Material.SelectableArticles.SelectedArticle.Dimensions.Thickness
                                        .Value.ToString(), NumberOnly, ""),
                            Unit =
                                s_lengthUnits[
                                    request.PartType.Material.SelectableArticles.SelectedArticle.Dimensions.Thickness
                                        .Value.Unit],
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
                        CommandString = "HANDLEFILE_BATCH",
                        Parameters = new[]
                        {
                             new Parameter { Id = "PARTTYPEID", Value = partTypeId },
                             new Parameter { Id = "OUTPUTPATH", Value =  cadmanSdiResultsFolder }
                        }
                    }
                }
            }
        };

        return response;
    }

    private string GetMaterialCode(RequestManufacturabilityCheckOfPartTypeMessage request)
    {
        var useMaterialMappingWithKeywords = _settings.MaterialMappingWithKeywords;

        if (useMaterialMappingWithKeywords)
        {
            var rhodiumMaterialKeywords = request.PartType.Material.SelectableArticles.SelectedArticle.Tokens.ToArray();
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
            var rhodiumMaterialId = request.PartType.Material.SelectableArticles.SelectedArticle.Id;
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
