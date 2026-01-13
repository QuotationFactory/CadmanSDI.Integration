using MediatR;

namespace CadmanSDI.Host.Features.CadmanSdiResultFile;

public class CadmanSdiResultFileCreated : INotification
{
    public string FilePath { get; }

    public CadmanSdiResultFileCreated(string filePath)
    {
        FilePath = filePath;
    }
}
