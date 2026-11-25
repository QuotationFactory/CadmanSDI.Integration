using CadmanSDI.Entities.Models;
using CadmanSDI.Entities.Models.V87;
using MetalHeaven.Agent.Shared.External.Messages;

namespace CadmanSDI.Import;

public class CadmanSdiImportMapperV87 : ICadmanSdiImportMapper
{
    public ICadmanSdiImport FromRequest(
        RequestManufacturabilityCheckOfPartTypeMessage request,
        string importFileName,
        string partTypeName,
        string materialId,
        string cadmanSdiResultsFolder,
        string macroName)
    {
        var values = CadmanSdiImportMappingValues.FromRequest(
            request, importFileName, partTypeName, materialId, macroName);

        return new CadManSdiImportV87
        {
            PartTypeImport = new PartTypeImport
            {
                PartTypeList = new PartTypeList
                {
                    PartTypes =
                    [
                        new PartType
                        {
                            Id = values.PartTypeName,
                            Comment = request.ProjectId.ToString(),
                            Description = request.PartType.Id.ToString(),
                            MaterialId = values.MaterialId,
                            Importfilename = importFileName,
                            Thickness = values.Thickness,
                            Unit = values.Unit
                        }
                    ]
                }
            },
            Batch = new Batch
            {
                Commands =
                [
                    new Command
                    {
                        CommandString = values.MacroName,
                        Parameters =
                        [
                            new Parameter { Id = "PARTTYPEID", Value = values.PartTypeName },
                            new Parameter { Id = "OUTPUTPATH", Value = cadmanSdiResultsFolder }
                        ]
                    }
                ]
            }
        };
    }
}
