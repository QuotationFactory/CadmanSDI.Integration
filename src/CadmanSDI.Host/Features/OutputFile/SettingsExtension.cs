using CadmanSDI.Entities;

namespace CadmanSDI.Host.Features.OutputFile;

public static class SettingsExtension
{
    public static string GetOrCreateAgentInputDirectory(this CadmanSdiIntegrationSettings settings, string integrationName = "", bool createIfNotExists = false)
    {
        var inputDirectory = Path.Combine(settings.RootDirectory, integrationName, "Input");
        return inputDirectory.DirectoryExistsOrCreate(createIfNotExists) ? inputDirectory : string.Empty;
    }

    public static string GetOrCreateAgentOutputDirectory(this CadmanSdiIntegrationSettings settings, string integrationName = "", bool createIfNotExists = false)
    {
        var outputDirectory = Path.Combine(settings.RootDirectory, integrationName, "Output");
        return outputDirectory.DirectoryExistsOrCreate(createIfNotExists) ? outputDirectory : string.Empty;
    }

    public static bool DirectoryExistsOrCreate(this string directory, bool createIfNotExists = false)
    {
        if (string.IsNullOrWhiteSpace(directory))
            return false;

        if (Directory.Exists(directory))
            return true;

        if (!createIfNotExists) return false;

        Directory.CreateDirectory(directory);

        return Directory.Exists(directory);
    }
    private static string GetOrCreateDirectory(string rootDirectoryPath, string subDir = "")
    {
        var totalPath = Path.Combine(rootDirectoryPath, subDir);

        if (Directory.Exists(totalPath))
            return totalPath;

        if (!Directory.Exists(totalPath))
            Directory.CreateDirectory(totalPath);

        return totalPath;
    }
    public static string GetInputDirectory(this CadmanSdiIntegrationSettings settings, string integrationName) => GetOrCreateDirectory(Path.Combine(settings.RootDirectory, integrationName, "Input"));
    public static string GetOutputDirectory(this CadmanSdiIntegrationSettings settings,string integrationName) => GetOrCreateDirectory(Path.Combine(settings.RootDirectory, integrationName, "Output"));

    private static string GetProcessingDirectory(this CadmanSdiIntegrationSettings settings, string integrationName) => GetOrCreateDirectory(settings.GetOutputDirectory(integrationName), "Processing");
    private static string GetProcessedDirectory(this CadmanSdiIntegrationSettings settings, string integrationName)
    {

        return GetOrCreateDirectory(settings.GetOutputDirectory(integrationName), "Processed");
    }

    private static string GetErrorDirectory(this CadmanSdiIntegrationSettings settings, string integrationName) => GetOrCreateDirectory(settings.GetOutputDirectory(integrationName), "Error");
    public static string MoveFileToProcessing(this CadmanSdiIntegrationSettings settings, string integrationName, string filePath) => filePath.MoveFileToDirectory(settings.GetProcessingDirectory(integrationName));
    public static string MoveFileToProcessed(this CadmanSdiIntegrationSettings settings, string integrationName, string filePath) => filePath.MoveFileToDirectory(settings.GetProcessedDirectory(integrationName));
    public static string MoveFileToError(this CadmanSdiIntegrationSettings settings, string integrationName, string filePath) => filePath.MoveFileToDirectory(settings.GetErrorDirectory(integrationName));
    public static string MoveFileToAgentInput(this CadmanSdiIntegrationSettings settings, string integrationName, string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var subDirectoryPath = settings.GetInputDirectory(integrationName);

        var destFileName = Path.Combine(subDirectoryPath, Path.GetFileName(filePath));

        File.Move(filePath, destFileName);

        return destFileName;
    }
    public static string MoveFileToDirectory(this string filePath, string directoryPath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var fileInfo = new FileInfo(filePath);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var destFileName = Path.Combine(directoryPath, fileInfo.Name);
        return filePath.MoveFile(destFileName);
    }
    private static string MoveFile(this string filePath, string destinationFilePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);

        var result = destinationFilePath;

        while (File.Exists(result))
        {
            result = Path.Combine(Path.GetDirectoryName(destinationFilePath) ?? "", $"{Path.GetFileNameWithoutExtension(destinationFilePath)} (1){Path.GetExtension(destinationFilePath)}");
        }

        File.Move(filePath, result);

        return result;
    }
}
