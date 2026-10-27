# Cadman SDI Integration

This Windows service connects the Edge connector to CADMAN-SDI. It imports
manufacturability requests into CADMAN-SDI, processes the resulting XML files,
and sends the response and configured documents back to the agent.

## Requirements

- .NET 10 SDK to build and run the service.
- Access to the Edge connector input and output folders.
- Access to CADMAN-SDI's watch and results folders.
- A CADMAN-SDI material mapping workbook at `CadmanSDI.Host\Data\MaterialMapping.xlsx`.

## Configure

Edit `CadmanSDI.Host\appsettings.json`, or provide equivalent configuration
through the .NET configuration system. Configure the `CadmanSdiIntegrationSettings`
section for the local installation.

| Setting | Purpose |
| --- | --- |
| `CadmanSdiVersion` | CADMAN-SDI version: `"8.7"` or `"8.9"`. Defaults to `"8.7"`; other values fail startup validation. |
| `IntegrationOutputFolder` | Agent output folder watched for incoming request JSON files. |
| `IntegrationInputFolder` | Agent input folder where response JSON files are written. |
| `CadmanSdiWatchFolder` | CADMAN-SDI watch folder where import XML files are written. |
| `CadmanSdiResultsFolder` | Folder watched for CADMAN-SDI result XML files. |
| `CadmanSdiResultsProcessedFolder` | Destination for processed result files. |
| `CadmanSdiStepFolder` | Folder where downloaded STEP files are stored. |
| `RootDirectory` | Root used by integration file-handling helpers. |
| `CadmanSdiMacroName` | Optional CADMAN-SDI macro name. When unset, the standard batch macro is used. |
| `SaveAsPartName` | Use the part name instead of its ID when naming the CADMAN-SDI part. |
| `SaveAsUniqueInclPartName` | Include the article number and part name in the CADMAN-SDI part name. |
| `MaterialMappingWithKeywords` | Map material by keywords when `true`, or by article ID when `false`. |
| `UploadDxfFile` | Include the generated DXF in the agent response when available. |
| `UploadSdiPrj` | Include the generated SDI project file in the response when available. |
| `UploadResultXmlFile` | Include the result XML in the response when enabled. |

Set folder paths to valid locations available to the service account.

## CADMAN-SDI version support

`CadmanSdiVersion` selects both the import XML mapper and result XML mapper.
Keep version-specific schema handling in the matching mapper and model.

| Version | Import IDs | Result IDs |
| --- | --- | --- |
| 8.7 | Project ID is written to `COMMENT`; part-type ID to `DESCRIPTION`. | Project ID and part-type ID are read from `Comment` and `Description` on the part model. If absent, the mapper falls back to the job order ID and result part-type ID. |
| 8.9 | Project ID and part-type ID are written as `TAGS` entries with `Type="ProjectId"` and `Type="PartTypeId"`. | The matching `TAG` values are preferred. If absent, the mapper falls back to the part model's `Comment` and `Description`, then the job order ID and result part-type ID. |

## Request and result flow

1. The host watches `IntegrationOutputFolder` for request JSON files.
2. For each request, it downloads the STEP file, creates a version-specific
   CADMAN-SDI import XML file, and places it in `CadmanSdiWatchFolder`.
3. The host watches `CadmanSdiResultsFolder` for result XML files and maps each
   result using the configured CADMAN-SDI version.
4. It writes the response JSON to `IntegrationInputFolder`. Enabled DXF, SDI
   project, and result XML documents are attached when present.
5. Result files are moved under `CadmanSdiResultsProcessedFolder` after handling.

Errors are logged. When project and part-type IDs can be recovered, the host
also writes an error response for the agent.

## Build, run, and test

Run these commands from this directory:

```powershell
dotnet restore QF.CadmanSDI.sln
dotnet build QF.CadmanSDI.sln --no-restore
dotnet test CadmanSDI.Tests\CadmanSDI.Tests.csproj --no-restore
dotnet run --project CadmanSDI.Host\CadmanSDI.Host.csproj
```

On Windows, the host supports running as a Windows Service. For local
development, it can also be started with `dotnet run`.
