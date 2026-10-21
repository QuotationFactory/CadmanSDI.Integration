using System.Xml;
using System.Xml.Serialization;
using CadmanSDI.Entities;
using CadmanSDI.Entities.Models;
using CadmanSDI.Entities.Repositories;
using CadmanSDI.Export;
using CadmanSDI.LocalData;
using CadmanSDI.LocalData.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CadmanSDI.Tests;

public class CadManSdiExportV87Tests
{
    private const string ResultSuccessfulPath =
        @"TestData\Test15062022-1.xml";
    private const string ResultFailedPath =
        @"TestData\XML afkeur.xml";

    [Fact]
    private async Task CreateRequestManufacturabilityCheckOfPartTypeMessageResponseFromResult_Successful()
    {
        // Arrange
        var settings = Mock.Of<IOptions<CadmanSdiIntegrationSettings>>(s =>
            s.Value.UploadDxfFile == true &&
            s.Value.MaterialMappingWithKeywords == true &&
            s.Value.SaveAsPartName == true &&
            s.Value.UploadResultXmlFile == true);

        ICadmanSdiRequestManufacturabilityResponseFactory manufacturabilityResponseFactory =
            new CadmanSdiRequestManufacturabilityResponseFactory(
                settings: settings,
                logger: new Mock<ILogger<CadmanSdiRequestManufacturabilityResponseFactory>>().Object);

        var result = GetDeserializedXml<CadManSdiExportV8_7>(ResultSuccessfulPath);

        // Act
        var response = await manufacturabilityResponseFactory.CreateFromResult(result, ResultSuccessfulPath);

        // Assert
        Assert.True(response.IsManufacturable);
        Assert.Contains(response.Documents, d => d.DocumentName.Contains(".dxf"));
        Assert.Contains(response.Documents, d => d.DocumentName.Contains(".xml"));
    }

    [Fact]
    private async Task CreateRequestManufacturabilityCheckOfPartTypeMessageResponseFromResult_Failed()
    {
        // Arrange
        var settings = Mock.Of<IOptions<CadmanSdiIntegrationSettings>>(s =>
            s.Value.UploadDxfFile == true &&
            s.Value.MaterialMappingWithKeywords == true &&
            s.Value.SaveAsPartName == true &&
            s.Value.UploadResultXmlFile == true);

        ICadmanSdiRequestManufacturabilityResponseFactory manufacturabilityResponseFactory =
            new CadmanSdiRequestManufacturabilityResponseFactory(
                settings: settings,
                logger: new Mock<ILogger<CadmanSdiRequestManufacturabilityResponseFactory>>().Object);

        var result = GetDeserializedXml<CadManSdiExportV8_7>(ResultFailedPath);

        // Act
        var response = await manufacturabilityResponseFactory.CreateFromResult(result, ResultFailedPath);

        // Assert
        Assert.False(response.IsManufacturable);
        Assert.Contains(response.Documents, d => d.DocumentName.Contains(".xml"));
    }

    [Fact]
    private async Task CreateRequestManufacturabilityCheckOfPartTypeMessageResponseFromResult_With_reason()
    {
        // Arrange

        var settings = Mock.Of<IOptions<CadmanSdiIntegrationSettings>>(s =>
            s.Value.UploadDxfFile == true &&
            s.Value.MaterialMappingWithKeywords == true &&
            s.Value.SaveAsPartName == true &&
            s.Value.UploadResultXmlFile == true);

        ICadmanSdiRequestManufacturabilityResponseFactory manufacturabilityResponseFactory =
            new CadmanSdiRequestManufacturabilityResponseFactory(
                settings: settings,
                logger: new Mock<ILogger<CadmanSdiRequestManufacturabilityResponseFactory>>().Object);

        var result = GetDeserializedXml<CadManSdiExportV8_7>(@"TestData\NotManufacturable\1a3bb73e-0ee0-479e-b6a5-19c28a5d0dfe.xml");

        // Act
        var response = await manufacturabilityResponseFactory.CreateFromResult(result, ResultFailedPath);

        // Assert
        Assert.False(response.IsManufacturable);
        Assert.Contains(response.Documents, d => d.DocumentName.Contains(".xml"));
    }

    [Fact]
    private void GetDeserializedSdiExport()
    {
        var sdiExport = GetDeserializedXml<CadManSdiExportV8_7>(ResultSuccessfulPath);

        Assert.NotNull(sdiExport.PARTTYPELIST.PARTSHEETMETALTYPE.MaterialID);
    }

    private static T GetDeserializedXml<T>(string filePath)
    {
        var xml = File.OpenRead(filePath);
        XmlDocument doc = new XmlDocument();
        doc.Load(xml);

        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using XmlReader reader = new XmlNodeReader(doc);
        return (T)serializer.Deserialize(reader)!;
    }
}
