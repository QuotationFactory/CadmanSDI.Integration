using System.Globalization;
using CadmanSDI.LocalData.Models;
using ClosedXML.Excel;

namespace CadmanSDI.LocalData;

public class MaterialMappingRepository : IMaterialMappingRepository
{
    public string GetMaterialIdFromArticle(string rhodiumMaterialId)
    {
        if (string.IsNullOrWhiteSpace(rhodiumMaterialId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(rhodiumMaterialId));
        }

        var onlyPath = Extensions.GetBasePath();

        var filePathName = Path.Combine(onlyPath, "data", "MaterialMapping.xlsx");

        if (!File.Exists(filePathName))
        {
            throw new Exception($"Data/MaterialMapping.xlsx not found.. Expected at: {filePathName}");
        }

        XLWorkbook workbook;
        using (var fileStream = new FileStream(filePathName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            workbook = new XLWorkbook(fileStream);
        }

        IXLWorksheet worksheet = workbook.Worksheets.FirstOrDefault() ?? throw new InvalidOperationException("No worksheet found in the workbook.");

        // select range with used data in the sheet
        var rangeUsed = worksheet.RangeUsed();

        // first column has the Rhodium Material id's
        if (rangeUsed != null)
        {
            var column1 = rangeUsed.Column(1);
            // search for the
            var foundCells = column1.Search(rhodiumMaterialId, CompareOptions.IgnoreCase);
            if (foundCells != null && foundCells.Any() )
            {
                var firstCell = foundCells.FirstOrDefault();
                if (firstCell != null)
                {
                    var rowNumber = firstCell.Address.RowNumber;
                    // get the cadman Material id
                    var column2 = rangeUsed.Column(2);
                    var cadmanMaterialId = column2.Cell(rowNumber:rowNumber).Value.ToString();
                    return cadmanMaterialId;
                }
            }
        }

        return string.Empty;
    }

    public string GetMaterialIdFromKeywords(string[] keywords)
    {
        if (!keywords.Any())
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(keywords));
        }

        var mappingList = GetMaterialMapListForGroupKeywordMapping();

        var cadmanbId = GetCadmanBMaterialIdFromKeywords(keywords, mappingList);

        return cadmanbId;
    }

    private static IEnumerable<MaterialMapModel> GetMaterialMapListForGroupKeywordMapping()
    {
        var onlyPath = Extensions.GetBasePath();

        var filePathName = Path.Combine(onlyPath, "data", "MaterialMapping.xlsx");

        if (!File.Exists(filePathName))
        {
            throw new Exception($"Data/MaterialMapping.xlsx not found.. Expected at: {filePathName}");
        }

        XLWorkbook workbook;
        using (var fileStream = new FileStream(filePathName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            workbook = new XLWorkbook(fileStream);
        }

        IXLWorksheet worksheet = workbook.Worksheets.FirstOrDefault() ?? throw new InvalidOperationException("No worksheet found in the workbook.");

        // select range with used data in the sheet
        var rangeUsed = worksheet.RangeUsed();

        var colMaterialGroup = 1;
        var colKeyword = 2;
        var colCadmanBId = 3;

        // Get all rows where all 3 cells have a value
        if (rangeUsed != null)
        {
            var mappingRows = rangeUsed.AsTable().DataRange.Rows()
                .Where(row => row.Cell(colMaterialGroup).GetString().Trim().Length > 0
                              && row.Cell(colKeyword).GetString().Trim().Length > 0
                              && row.Cell(colCadmanBId).GetString().Trim().Length > 0);

            var mappingList = new List<MaterialMapModel>();
            foreach (var xlTableRow in mappingRows)
            {
                var newMapping = new MaterialMapModel
                {
                    MaterialGroup = xlTableRow.Cell(colMaterialGroup).Value.ToString(),
                    Keyword = xlTableRow.Cell(colKeyword).Value.ToString(),
                    CadmanBId = xlTableRow.Cell(colCadmanBId).Value.ToString()
                };
                mappingList.Add(newMapping);
            }

            return mappingList;
        }
        return new List<MaterialMapModel>();
    }

    private static string GetCadmanBMaterialIdFromKeywords(string[] keywords, IEnumerable<MaterialMapModel> mappingList)
    {
        var lowerKeywords = keywords.Select(keyword => keyword.ToLower());
        // Find mapping rows where the keywords map with the material group
        var inGroup = mappingList.Where(m => lowerKeywords.Contains(m.MaterialGroup.ToLower())).ToList();
        if (inGroup.Any())
        {
            // Next find the rows where the mapping-keyword maps with the keywords
            var foundKeyword = inGroup.Where(k => lowerKeywords.Contains(k.Keyword.ToLower())).ToList();
            if (foundKeyword.Any())
            {
                return foundKeyword.First().CadmanBId;
            }
        }

        return string.Empty;
    }
}
