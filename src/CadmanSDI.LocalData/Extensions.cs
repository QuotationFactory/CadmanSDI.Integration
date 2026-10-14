using System.Diagnostics;

namespace CadmanSDI.LocalData;

public static class Extensions
{
    public static string GetBasePath()
    {
        var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
        var executable = Path.GetFileNameWithoutExtension(executablePath);

        if ("dotnet".Equals(executable, StringComparison.InvariantCultureIgnoreCase))
        {
            return Path.GetDirectoryName(typeof(ManufacturabilityRequestRepository).Assembly.Location) ?? string.Empty;
        }

        return Path.GetDirectoryName(executablePath) ?? string.Empty;
    }
}
