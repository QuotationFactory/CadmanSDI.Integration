using System.Diagnostics;
using System.Runtime.InteropServices;
using CadmanSDI.Host.Features;
using CadmanSDI.Host.Features.OutputFile;
using CadmanSDI.Host.Features.ProcessResult;
using CadmanSDI.Host.Features.RFM;
using CadmanSDI.Host.Features.RFMR;
using CadmanSDI.LocalData;
using MetalHeaven.Agent.Shared.External.Classes;
using MetalHeaven.Agent.Shared.External.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CadmanSDI.Host;

public static class Program
{
    public static void Main(string[] args)
    {

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
            Log.Information("Starting host. Version: {Version}", version);
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            hostBuilder.UseWindowsService();
        }
        // else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        // {
        //     // https://devblogs.microsoft.com/dotnet/net-core-and-systemd/
        //     // Requires Microsoft.Extensions.Hosting.Systemd
        //     hostBuilder.UseSystemd();
        // }

        hostBuilder.UseContentRoot(GetBasePath());
        hostBuilder.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithEnvironmentUserName()
                .WriteTo.File(Path.Combine(GetBasePath(), "Logs", "log.txt"), rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj} {NewLine}{Exception}")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj} {NewLine}{Exception}")
            );

        hostBuilder.ConfigureServices((hostContext, services) =>
        {
            services.AddCadmanSdiImport(hostContext.Configuration);
            services.AddCadmanSdiResult(hostContext.Configuration);

            // register agent message serialization helper
            services.AddTransient<IAgentMessageSerializationHelper, ExternalAgentMessageSerializationHelper>();

            services.AddTransient<IRequestForManufacturability, RequestForManufacturability>();
            services.AddTransient<ICadmanSdiResult, CadmanSdiResultHandler>();
            services.AddTransient<IRequestManufacturabilityResponse, RequestManufacturabilityResponse>();
            services.AddTransient<IManufacturabilityRequestRepository, ManufacturabilityRequestRepository>();
            services.AddTransient<IMaterialMappingRepository, MaterialMappingRepository>();

            // register file processing handlers and watcher services
            services.AddScoped<OutputFileCreatedHandler>();
            services.AddScoped<CadmanSdiResultFileCreatedHandler>();
            services.AddHostedService<OutputFileWatcherService>();
            services.AddHostedService<CadmanSdiResultFileWatcherService>();
        });
        return hostBuilder;
    }


    private static string GetBasePath()
    {
        var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
        var executable = Path.GetFileNameWithoutExtension(executablePath);

        if ("dotnet".Equals(executable, StringComparison.InvariantCultureIgnoreCase))
        {
            return Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? string.Empty;
        }

        return Path.GetDirectoryName(executablePath) ?? string.Empty;
    }
}
