using System.Runtime.InteropServices;

namespace CSCanbulatEngine.FileHandling;

public class FileHandling
{
    public static string GetNameOfFile(string entireDirectory)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? entireDirectory.Split('\\').Last()
            : entireDirectory.Split("/").Last();
    }
}