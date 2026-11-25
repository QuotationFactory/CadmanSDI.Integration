using CadmanSDI.Host.Features;
using CadmanSDI.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CadmanSDI.Tests;

public class CadmanSdiResultMappingTests
{
    [Theory]
    [InlineData("8.7", typeof(CadmanSdiResultMapperV87))]
    [InlineData("8.9", typeof(CadmanSdiResultMapperV89))]
    public void ConfigurationSelectsResultMapper(string version, Type mapperType)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CadmanSdiIntegrationSettings:RootDirectory"] = @"C:\Agent",
                ["CadmanSdiIntegrationSettings:CadmanSdiVersion"] = version
            })
            .Build();
        var services = new ServiceCollection();
        services.AddCadmanSdiResult(configuration);
        using var provider = services.BuildServiceProvider();

        Assert.IsType(mapperType, provider.GetRequiredService<ICadmanSdiResultMapper>());
    }

    [Fact]
    public void V87ReadsProjectAndPartTypeIdsFromCommentAndDescription()
    {
        var result = Map(new CadmanSdiResultMapperV87(), includeTags: false);

        Assert.Equal(ProjectId, result.ProjectId);
        Assert.Equal(PartTypeId, result.PartTypeId);
        Assert.Equal("Finished", result.JobStatus);
        Assert.Equal("Finished", result.PreparationJobStatus);
        Assert.Equal("1", result.BendingExecutionValid);
    }

    [Fact]
    public void V89ReadsProjectAndPartTypeIdsFromTags()
    {
        var result = Map(new CadmanSdiResultMapperV89(), includeTags: true);

        Assert.Equal(ProjectId, result.ProjectId);
        Assert.Equal(PartTypeId, result.PartTypeId);
        Assert.Equal("Finished", result.JobStatus);
        Assert.Equal("Finished", result.PreparationJobStatus);
        Assert.Equal("1", result.BendingExecutionValid);
    }

    private const string ProjectId = "97f3a2bc-5862-463b-9cdd-0e4005be86ba";
    private const string PartTypeId = "8190670b-6dcd-440b-a49f-b056b9d81af2";

    private static CadmanSdiResultData Map(ICadmanSdiResultMapper mapper, bool includeTags)
    {
        var comment = includeTags ? "00000000-0000-0000-0000-000000000001" : ProjectId;
        var description = includeTags ? "00000000-0000-0000-0000-000000000002" : PartTypeId;
        var tags = includeTags
            ? $"<TAGS><TAG Type=\"ProjectId\">{ProjectId}</TAG><TAG Type=\"PartTypeId\">{PartTypeId}</TAG></TAGS>"
            : "<TAGS/>";
        var xml = $"""
            <CADMAN>
              <JOBOSMCONVERTLIST><JOBOSMCONVERT><JobStatus>Finished</JobStatus></JOBOSMCONVERT></JOBOSMCONVERTLIST>
              <JOBPARTPREPARATIONLIST><JOBPARTPREPARATION><JobStatus>Finished</JobStatus></JOBPARTPREPARATION></JOBPARTPREPARATIONLIST>
              <PARTTYPELIST><PARTSHEETMETALTYPE><PARTSHEETMETALMODELLIST><PARTSHEETMETALMODEL>
                <Description>{description}</Description><Comment>{comment}</Comment>{tags}
              </PARTSHEETMETALMODEL></PARTSHEETMETALMODELLIST></PARTSHEETMETALTYPE></PARTTYPELIST>
              <EXECUTIONPLANPROPOSALLIST><EXECUTIONPLANPROPOSAL><PATHLIST><PATH><STEPLIST>
                <STEP Kind="BENDINGEXEC" Valid="1"><JOBTYPELIST><JOBTYPE><SUMMARY><Valid>1</Valid></SUMMARY></JOBTYPE></JOBTYPELIST></STEP>
              </STEPLIST></PATH></PATHLIST></EXECUTIONPLANPROPOSAL></EXECUTIONPLANPROPOSALLIST>
            </CADMAN>
            """;
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        try
        {
            File.WriteAllText(path, xml);
            return mapper.FromResultFile(path);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
