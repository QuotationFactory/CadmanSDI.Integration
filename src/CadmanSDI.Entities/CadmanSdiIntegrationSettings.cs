namespace CadmanSDI.Entities;

public class CadmanSdiIntegrationSettings
{
    public string CadmanSdiVersion { get; set; } = "8.7";

    public required string RootDirectory { get; set; }

    // The folder where the Cadman SDI saves its downloads.
    public string IntegrationOutputFolder { get; set; }

    // The folder where the Cadman SDI watches for files to upload.
    public string IntegrationInputFolder { get; set; }

    // The folder where  Cadman SDI watches for files to process.
    public string CadmanSdiWatchFolder { get; set; }

    // The folder where Cadman SDI saves processed results.
    public string CadmanSdiResultsFolder { get; set; }

    // The name of the macro in Cadman SDI to run for processing the files.
    public string CadmanSdiMacroName { get; set; }

    // Save the project in Cadman SDI as the part-name (instead of the part-id).
    public bool SaveAsPartName { get; set; }

    // Save the project in Cadman SDI as the part-name (instead of the part-id).
    public bool SaveAsUniqueInclPartName { get; set; }

    // If true, maps the material based on material group and keyword(s).
    // If false, maps the material based on the article ID.
    public bool MaterialMappingWithKeywords { get; set; }

    // Upload DXF file created by Cadman SDI.
    public bool UploadDxfFile { get; set; }

    // Upload SDIPrj file created by Cadman SDI.
    public bool UploadSdiPrj { get; set; }

    // Upload CBBatchResult file created by Cadman SDI.
    public bool UploadResultXmlFile { get; set; }
    //  The folder where the original stepfile will be saved before processing in Cadman SDI.
    public string CadmanSdiStepFolder { get; set; }
    public string CadmanSdiResultsProcessedFolder { get; set; }
}
