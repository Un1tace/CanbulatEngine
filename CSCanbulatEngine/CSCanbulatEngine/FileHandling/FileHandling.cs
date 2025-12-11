using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSCanbulatEngine.FileHandling;

/// <summary>
/// Class to help with file handling
/// </summary>
public class FileHandling
{
    /// <summary>
    /// Get name of file with extension from entire filePath
    /// </summary>
    /// <param name="entireDirectory">File path</param>
    /// <returns>Name</returns>
    public static string GetNameOfFile(string entireDirectory)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? entireDirectory.Split('\\').Last()
            : entireDirectory.Split("/").Last();
    }

    /// <summary>
    /// Open filepath to where its stored
    /// </summary>
    /// <param name="path">Path to file</param>
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
            EngineLog.Log($"Error showing file in manager: {e.Message}");
            throw;
        }
    }

    public static string GetPath(string relativePath)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;

        string fullPath = Path.Combine(basePath, relativePath);
        
        return fullPath;
    }
}