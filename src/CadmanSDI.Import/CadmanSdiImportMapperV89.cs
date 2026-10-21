using CadmanSDI.Entities.Models;
using CadmanSDI.Entities.Models.V89;
using MetalHeaven.Agent.Shared.External.Messages;

namespace CadmanSDI.Import;

public class CadmanSdiImportMapperV89 : ICadmanSdiImportMapper
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

        return new CadManSdiImportV89
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
                            MaterialId = values.MaterialId,
                            Importfilename = importFileName,
                            Thickness = values.Thickness,
                            Unit = values.Unit,
                            Tags = new Tags()
                            {
                                TagsList =
                                [

                                    new Tag()
                                    {
                                        Type = "ProjectId",
                                        Text = request.ProjectId.ToString()
                                    },
                                    new Tag()
                                    {
                                        Type = "PartTypeId",
                                        Text = request.PartType.Id.ToString()
                                    }
                                ]
                            }
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
