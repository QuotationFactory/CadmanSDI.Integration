using System.Globalization;
using System.Text.RegularExpressions;
using MetalHeaven.Agent.Shared.External.Messages;
using UnitsNet.Units;

namespace CadmanSDI.Import;

internal sealed partial record CadmanSdiImportMappingValues(
    string PartTypeName,
    string MaterialId,
    string MacroName,
    string? Thickness,
    string? Unit)
{
    private static readonly Dictionary<LengthUnit, string> s_lengthUnits = new()
    {
        { LengthUnit.Millimeter, "MM" }
    };

    public static CadmanSdiImportMappingValues FromRequest(
        RequestManufacturabilityCheckOfPartTypeMessage request,
        string importFileName,
        string partTypeName,
        string materialId,
        string macroName)
    {
        if (string.IsNullOrWhiteSpace(importFileName))
            throw new ArgumentNullException(nameof(importFileName));

        var selectedArticle = request.PartType.Material.SelectableArticles.SelectedArticle;
        if (string.IsNullOrWhiteSpace(partTypeName))
            partTypeName = request.PartType.Name;

        if (string.IsNullOrWhiteSpace(materialId) && selectedArticle != null)
            materialId = selectedArticle.Id;

        string? thicknessValue = null;
        string? unit = null;
        if (selectedArticle != null)
        {
            var thickness = selectedArticle.Dimensions.Thickness ?? default;
            thicknessValue = NumberOnlyRegex().Replace(thickness.Value.ToString(CultureInfo.InvariantCulture), "");
            unit = s_lengthUnits[thickness.Unit];
        }

        return new CadmanSdiImportMappingValues(
            partTypeName,
            materialId,
            string.IsNullOrWhiteSpace(macroName) ? "HANDLEFILE_BATCH" : macroName,
            thicknessValue,
            unit);
    }

    [GeneratedRegex("[^0-9.]")]
    private static partial Regex NumberOnlyRegex();
}
