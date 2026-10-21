using System.Xml;
using System.Xml.Serialization;
using CadmanSDI.Entities.Models;
using CadmanSDI.Entities.Models.V8_7;
using CadmanSDI.Import;
using MetalHeaven.Agent.Shared.External.Messages;
using Newtonsoft.Json;
using Versioned.ExternalDataContracts;
using Xunit;

namespace CadmanSDI.Tests;

public class CadManSdiImportV87Tests
{
    private const string SdiInputPath = @"TestData\Cadman-SDI input.xml";
    private const string ManufacturabilityRequestPath = @"TestData/manufacturabilityRequest.json";

    [Fact]
    private void GetDeserializedSdiImport()
    {
        var sdiImport = GetDeserializedXml<CadManSdiImportV8_7>(SdiInputPath);

        Assert.Contains("demoLVD.sat", sdiImport.PartTypeImport.PartTypeList.PartTypes[0].Importfilename);
    }


    private RequestManufacturabilityCheckOfPartTypeMessage GetDeserializedManufacturabilityCheckOfPartTypeMessage()
    {
        // define json serializer settings
        var settings = new JsonSerializerSettings();
        settings.SetJsonSettings();
        settings.AddJsonConverters();
        // settings.SerializationBinder = new CrossPlatformTypeBinder();

        // read all text from file that is created
        var json = File.ReadAllText(ManufacturabilityRequestPath);

        // convert json to project object
        return JsonConvert.DeserializeObject<RequestManufacturabilityCheckOfPartTypeMessage>(json, settings)!;
    }

    private ICadmanSdiImport GetSdiImportFromManufacturabilityCheckOfPartTypeMessage()
    {
        var request = GetDeserializedManufacturabilityCheckOfPartTypeMessage();

        var partTypeId = request.PartType.Name;

        var materialId = request.PartType.Material.SelectableArticles.SelectedArticle?.Id ?? string.Empty;

        var cadmanSdiResultsFolder = string.Empty;

        return CadmanSdiImportFactory.FromRequest(request, "testSdiImport.xml", partTypeId, materialId, cadmanSdiResultsFolder, string.Empty);
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

    [Fact]
    private void WriteSdiImport()
    {
        var sdiImport = GetSdiImportFromManufacturabilityCheckOfPartTypeMessage();

        var serializer = new XmlSerializer(typeof(CadManSdiImportV8_7));

        var file = File.Create("SdiImportTest.xml");
        serializer.Serialize(file, sdiImport);
        file.Close();

        Assert.NotNull(GetDeserializedXml<CadManSdiImportV8_7>(file.Name));
    }
}
