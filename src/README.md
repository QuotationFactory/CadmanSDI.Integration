# CadmanSDI.Integration

## CADMAN-SDI import version

Set `CadmanSdiIntegrationSettings.CadmanSdiVersion` in
`CadmanSDI.Host\appsettings.json` to `"8.7"` or `"8.9"` and restart the host.
Omitting the setting defaults to `"8.7"`; unsupported values fail startup.
This setting selects the import XML mapper only, not result parsing.

`CadmanSdiImportFactory` prepares filenames and material mappings, then delegates
to `CadmanSdiImportMapperV87` or `CadmanSdiImportMapperV89`. Each mapper builds
its own version-specific XML model, and the host serializes that concrete model.
Keep schema-specific changes in the corresponding mapper and model.
Version 8.7 stores the request project ID in `COMMENT` and part-type ID in
`DESCRIPTION`. Version 8.9 stores these IDs in `TAGS`, using `TAG` entries with
`Type="ProjectId"` and `Type="PartTypeId"` respectively, and omits `COMMENT`
and `DESCRIPTION`.