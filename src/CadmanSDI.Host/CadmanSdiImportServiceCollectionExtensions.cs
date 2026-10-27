using CadmanSDI.Entities;
using CadmanSDI.Import;
using CadmanSDI.Result;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CadmanSDI.Host;

public static class CadmanSdiImportServiceCollectionExtensions
{
    public static IServiceCollection AddCadmanSdiImport(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<CadmanSdiIntegrationSettings>()
            .Bind(configuration.GetSection("CadmanSdiIntegrationSettings"))
            .ValidateDataAnnotations()
            .Validate(
                settings => settings.CadmanSdiVersion is "8.7" or "8.9",
                "CadmanSdiVersion must be '8.7' or '8.9'.")
            .ValidateOnStart();

        services.AddTransient<ICadmanSdiImportFactory, CadmanSdiImportFactory>();
        services.AddTransient<CadmanSdiResultMapperV87>();
        services.AddTransient<CadmanSdiResultMapperV89>();
        services.AddTransient<ICadmanSdiResultMapper>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<CadmanSdiIntegrationSettings>>().Value;
            return settings.CadmanSdiVersion switch
            {
                "8.7" => provider.GetRequiredService<CadmanSdiResultMapperV87>(),
                "8.9" => provider.GetRequiredService<CadmanSdiResultMapperV89>(),
                _ => throw new InvalidOperationException(
                    $"Unsupported CADMAN-SDI version: '{settings.CadmanSdiVersion}'.")
            };
        });
        services.AddTransient<CadmanSdiImportMapperV87>();
        services.AddTransient<CadmanSdiImportMapperV89>();
        services.AddTransient<ICadmanSdiImportMapper>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<CadmanSdiIntegrationSettings>>().Value;
            return settings.CadmanSdiVersion switch
            {
                "8.7" => provider.GetRequiredService<CadmanSdiImportMapperV87>(),
                "8.9" => provider.GetRequiredService<CadmanSdiImportMapperV89>(),
                _ => throw new InvalidOperationException(
                    $"Unsupported CADMAN-SDI version: '{settings.CadmanSdiVersion}'.")
            };
        });

        return services;
    }
}
