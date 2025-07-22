using System.Diagnostics;
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

    public static void ShowInFileManager(string path)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            else
            {
                Process.Start("open", $"-R \"{path}\"");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error showing file in manager: {e.Message}");
            throw;
        }
    }
}