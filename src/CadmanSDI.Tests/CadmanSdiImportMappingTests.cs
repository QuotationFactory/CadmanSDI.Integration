using System.Globalization;
using System.Xml.Linq;
using System.Xml.Serialization;
using CadmanSDI.Entities;
using CadmanSDI.Entities.Models;
using CadmanSDI.Entities.Models.V87;
using CadmanSDI.Entities.Models.V89;
using CadmanSDI.Entities.Repositories;
using CadmanSDI.Host;
using CadmanSDI.Import;
using MetalHeaven.Agent.Shared.External.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using UnitsNet;
using Xunit;

namespace CadmanSDI.Tests;

public class CadmanSdiImportMappingTests
{
    [Theory]
    [InlineData(null, typeof(CadmanSdiImportMapperV87), typeof(CadManSdiImportV87))]
    [InlineData("8.7", typeof(CadmanSdiImportMapperV87), typeof(CadManSdiImportV87))]
    [InlineData("8.9", typeof(CadmanSdiImportMapperV89), typeof(CadManSdiImportV89))]
    public void ConfigurationSelectsMapperAndSerializableModel(
        string? version, Type mapperType, Type modelType)
    {
        using var provider = CreateProvider(version);
        provider.GetRequiredService<IStartupValidator>().Validate();
        var mapper = provider.GetRequiredService<ICadmanSdiImportMapper>();
        Assert.IsType(mapperType, mapper);
        var request = CreateRequest();

        var import = mapper.FromRequest(request, @"C:\Input\part.step", "part", "steel",
            @"C:\Results", "CUSTOM_MACRO");

        Assert.IsType(modelType, import);
        var xml = Serialize(import);
        Assert.Equal("CADMAN", xml.Name.LocalName);
        var part = xml.Element("PARTTYPEIMPORT")!.Element("PARTTYPELIST")!.Element("PARTTYPE")!;
        Assert.Equal("part", (string?)part.Attribute("ID"));
        if (version == "8.9")
        {
            Assert.Null(part.Element("COMMENT"));
            Assert.Null(part.Element("DESCRIPTION"));
            var tagsElement = part.Element("TAGS");
            Assert.NotNull(tagsElement);
            var tags = tagsElement.Elements("TAG")
                .ToDictionary(tag => (string)tag.Attribute("Type")!, tag => tag.Value);
            Assert.Equal(2, tags.Count);
            Assert.Equal(request.ProjectId.ToString(), tags["ProjectId"]);
            Assert.Equal(request.PartType.Id.ToString(), tags["PartTypeId"]);
        }
        else
        {
            Assert.Equal(request.ProjectId.ToString(), (string?)part.Element("COMMENT"));
            Assert.Equal(request.PartType.Id.ToString(), (string?)part.Element("DESCRIPTION"));
            Assert.Null(part.Element("TAGS"));
        }
        Assert.Equal("steel", (string?)part.Element("MaterialID"));
        Assert.Equal(@"C:\Input\part.step", (string?)part.Element("IMPORTFILENAME"));
        Assert.Equal("3", (string?)part.Element("THICKNESS"));
        Assert.Equal("MM", (string?)part.Element("UNIT"));
        var command = xml.Element("BATCH")!.Element("COMMAND")!;
        Assert.Equal("CUSTOM_MACRO", (string?)command.Attribute("Command"));
        var parameters = command.Element("PARAMETERS")!.Elements("PARAMETER")
            .ToDictionary(p => (string)p.Attribute("ID")!, p => (string)p.Attribute("Value")!);
        Assert.Equal(2, parameters.Count);
        Assert.Equal("part", parameters["PARTTYPEID"]);
        Assert.Equal(@"C:\Results", parameters["OUTPUTPATH"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("8.8")]
    [InlineData("invalid")]
    public void UnsupportedVersionFailsStartupValidation(string version)
    {
        using var provider = CreateProvider(version);
        var exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IStartupValidator>().Validate());
        Assert.Contains("CadmanSdiVersion must be '8.7' or '8.9'.", exception.Failures);
    }

    [Theory]
    [InlineData("8.7")]
    [InlineData("8.9")]
    public void MapperPreservesFallbacksAndInvariantThickness(string version)
    {
        using var provider = CreateProvider(version);
        var request = CreateRequest();
        request.PartType.Material.SelectableArticles.SelectedArticle!.Dimensions.Thickness =
            Length.FromMillimeters(2.5);
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("nl-NL");
            var import = provider.GetRequiredService<ICadmanSdiImportMapper>()
                .FromRequest(request, "part.step", "", "", "", "");
            var xml = Serialize(import);
            var part = xml.Descendants("PARTTYPE").Single();
            Assert.Equal(request.PartType.Name, (string?)part.Attribute("ID"));
            Assert.Equal(request.PartType.Material.SelectableArticles.SelectedArticle.Id,
                (string?)part.Element("MaterialID"));
            Assert.Equal("2.5", (string?)part.Element("THICKNESS"));
            Assert.Equal("HANDLEFILE_BATCH", (string?)xml.Descendants("COMMAND").Single().Attribute("Command"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Theory]
    [InlineData("8.7")]
    [InlineData("8.9")]
    public void MapperOmitsDimensionsWhenNoArticleIsSelected(string version)
    {
        using var provider = CreateProvider(version);
        var request = CreateRequest();
        request.PartType.Material.SelectableArticles.SelectedArticle = null;
        var import = provider.GetRequiredService<ICadmanSdiImportMapper>()
            .FromRequest(request, "part.step", "part", "mapped", "", "");
        var part = Serialize(import).Descendants("PARTTYPE").Single();
        Assert.Equal("mapped", (string?)part.Element("MaterialID"));
        Assert.Null(part.Element("THICKNESS"));
        Assert.Null(part.Element("UNIT"));
    }

    [Theory]
    [InlineData("8.7", null)]
    [InlineData("8.7", "")]
    [InlineData("8.7", " ")]
    [InlineData("8.9", null)]
    [InlineData("8.9", "")]
    [InlineData("8.9", " ")]
    public void MapperRejectsMissingImportFilename(string version, string? filename)
    {
        using var provider = CreateProvider(version);
        var exception = Assert.Throws<ArgumentNullException>(() => provider
            .GetRequiredService<ICadmanSdiImportMapper>()
            .FromRequest(CreateRequest(), filename!, "part", "material", "", ""));
        Assert.Equal("importFileName", exception.ParamName);
    }

    [Theory]
    [InlineData("8.7", false, false, false)]
    [InlineData("8.9", false, false, false)]
    [InlineData("8.7", true, false, true)]
    [InlineData("8.9", true, false, true)]
    [InlineData("8.7", true, true, false)]
    [InlineData("8.9", true, true, false)]
    public void FactoryPreservesNamingAndMaterialMapping(
        string version, bool saveAsName, bool uniqueName, bool keywords)
    {
        var repository = new Mock<IMaterialMappingRepository>(MockBehavior.Strict);
        var request = CreateRequest();
        request.PartType.Name = "part:name";
        request.PartType.SellingPartyArticleNumber = "article";
        if (keywords)
            repository.Setup(r => r.GetMaterialIdFromKeywords(
                It.Is<string[]>(tokens => tokens.SequenceEqual(
                    request.PartType.Material.SelectableArticles.SelectedArticle!.Tokens))))
                .Returns("mapped-material");
        else
            repository.Setup(r => r.GetMaterialIdFromArticle(
                    request.PartType.Material.SelectableArticles.SelectedArticle!.Id))
                .Returns("mapped-material");

        using var provider = CreateProvider(version, services =>
        {
            services.AddLogging();
            services.AddSingleton(repository.Object);
            services.Configure<CadmanSdiIntegrationSettings>(settings =>
            {
                settings.SaveAsPartName = saveAsName;
                settings.SaveAsUniqueInclPartName = uniqueName;
                settings.MaterialMappingWithKeywords = keywords;
            });
        });
        var import = provider.GetRequiredService<ICadmanSdiImportFactory>().FromRequest(request, "part.step");
        var part = Serialize(import).Descendants("PARTTYPE").Single();
        var expectedName = uniqueName ? "article_part_name"
            : saveAsName ? "part_name" : request.PartType.Id.ToString();
        Assert.Equal(expectedName, (string?)part.Attribute("ID"));
        Assert.Equal("mapped-material", (string?)part.Element("MaterialID"));
        repository.VerifyAll();
    }

    private static ServiceProvider CreateProvider(string? version, Action<IServiceCollection>? configure = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["CadmanSdiIntegrationSettings:RootDirectory"] = @"C:\Agent"
        };
        if (version != null)
            values["CadmanSdiIntegrationSettings:CadmanSdiVersion"] = version;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddCadmanSdiImport(configuration);
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    private static RequestManufacturabilityCheckOfPartTypeMessage CreateRequest()
    {
        return new RequestManufacturabilityCheckOfPartTypeMessage
        {
            ProjectId = Guid.Parse("97f3a2bc-5862-463b-9cdd-0e4005be86ba"),
            PartType = new()
            {
                Id = Guid.Parse("8190670b-6dcd-440b-a49f-b056b9d81af2"),
                Name = "part",
                Material = new()
                {
                    SelectableArticles = new()
                    {
                        SelectedArticle = new()
                        {
                            Id = "steel-3mm",
                            Tokens = ["sheet", "steel"],
                            Dimensions = new() { Thickness = Length.FromMillimeters(3) }
                        }
                    }
                }
            }
        };
    }

    private static XElement Serialize(ICadmanSdiImport import)
    {
        var serializer = new XmlSerializer(import.GetType());
        using var writer = new StringWriter();
        serializer.Serialize(writer, import);
        using var reader = new StringReader(writer.ToString());
        Assert.IsType(import.GetType(), serializer.Deserialize(reader));
        return XElement.Parse(writer.ToString());
    }
}
