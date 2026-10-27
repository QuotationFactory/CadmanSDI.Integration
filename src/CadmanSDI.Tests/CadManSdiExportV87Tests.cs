using System.Xml;
using System.Xml.Serialization;
using CadmanSDI.Entities;
using CadmanSDI.Entities.Models.V87;
using CadmanSDI.Host.Features.RFMR;
using CadmanSDI.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CadmanSDI.Tests;

public class CadManSdiExportV87Tests
{
    private const string ResultSuccessfulPath =
        @"TestData\XML goed.xml";
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

        IRequestManufacturabilityResponse manufacturabilityResponse =
            new RequestManufacturabilityResponse(
                settings: settings,
                logger: new Mock<ILogger<RequestManufacturabilityResponse>>().Object);

        var result = new CadmanSdiResultMapperV87().FromResultFile(ResultSuccessfulPath);

        // Act
        var response = await manufacturabilityResponse.CreateFromResult(result, ResultSuccessfulPath);

        // Assert
        Assert.True(response.IsManufacturable);
        Assert.Contains(response.Documents, d => d.DocumentName.Contains(".dxf"));
        Assert.Contains(response.Documents, d => d.DocumentName.Contains(".xml"));
    }

    [Fact]
    private async Task CreateRequestManufacturabilityCheckOfPartTypeMessageResponseFromResult_WithoutIds()
    {
        // Arrange
        var settings = Mock.Of<IOptions<CadmanSdiIntegrationSettings>>(s =>
            s.Value.UploadDxfFile == true &&
            s.Value.MaterialMappingWithKeywords == true &&
            s.Value.SaveAsPartName == true &&
            s.Value.UploadResultXmlFile == true);

        IRequestManufacturabilityResponse manufacturabilityResponse =
            new RequestManufacturabilityResponse(
                settings: settings,
                logger: new Mock<ILogger<RequestManufacturabilityResponse>>().Object);

        var result = new CadmanSdiResultMapperV87().FromResultFile(ResultFailedPath);

        // Act
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => manufacturabilityResponse.CreateFromResult(result, ResultFailedPath));
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

        IRequestManufacturabilityResponse manufacturabilityResponse =
            new RequestManufacturabilityResponse(
                settings: settings,
                logger: new Mock<ILogger<RequestManufacturabilityResponse>>().Object);

        var result = new CadmanSdiResultMapperV87()
            .FromResultFile(@"TestData\NotManufacturable\1a3bb73e-0ee0-479e-b6a5-19c28a5d0dfe.xml");

        // Act
        var response = await manufacturabilityResponse.CreateFromResult(result, ResultFailedPath);

        // Assert
        Assert.False(response.IsManufacturable);
        Assert.Contains(response.Documents, d => d.DocumentName.Contains(".xml"));
    }

    [Fact]
    private void GetDeserializedSdiExport()
    {
        var sdiExport = GetDeserializedXml<CadManSdiExportV87>(ResultSuccessfulPath);

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
