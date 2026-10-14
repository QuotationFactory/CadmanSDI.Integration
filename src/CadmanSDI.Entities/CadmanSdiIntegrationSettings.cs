namespace CadmanSDI.Entities;

public class CadmanSdiIntegrationSettings
{
    public required string RootDirectory { get; set; }

    // The folder where the MetalHeaven agent saves it's downloads.
    public string IntegrationOutputFolder { get; set; }

    // The folder where the MetalHeaven agent watches for files to upload.
    public string IntegrationInputFolder { get; set; }

    // The folder where CADMAN-SDI watches for files to process.
    public string CadmanSdiWatchFolder { get; set; }

    // The folder where CADMAN-SDI saves processed results.
    public string CadmanSdiResultsFolder { get; set; }

    // The name of the macro in CADMAN-SDI to run for processing the files.
    public string CadmanSdiMacroName { get; set; }

    // Save the project in cadman-sdi as the part-name (instead of the part-id).
    public bool SaveAsPartName { get; set; }

    // Save the project in cadman-sdi as the part-name (instead of the part-id).
    public bool SaveAsUniqueInclPartName { get; set; }

    // If true, maps the material based on material group and keyword(s).
    // If false, maps the material based on the article ID.
    public bool MaterialMappingWithKeywords { get; set; }

    // Upload DXF file created by cadman-sdi.
    public bool UploadDxfFile { get; set; }

    // Upload SDIPrj file created by cadman-sdi.
    public bool UploadSdiPrj { get; set; }

    // Upload CBBatchResult file created by cadman-sdi.
    public bool UploadResultXmlFile { get; set; }
    public string CadmanSdiStepFolder { get; set; }
    public string CadmanSdiResultsProcessedFolder { get; set; }
}
