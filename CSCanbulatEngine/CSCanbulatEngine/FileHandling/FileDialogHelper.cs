using TinyDialogsNet;
namespace CSCanbulatEngine.FileHandling;

public static class FileDialogHelper
{
    public static string? ShowOpenFileDialog(string defaultPath)
    {
        string[] filterPatterns = { ".cbe" };
        FileFilter ff = new FileFilter("Canbulat Engine Scene (*.cbe)", filterPatterns);

        var result = TinyDialogs.OpenFileDialog(
            title: "Open Scene",
            defaultPath: defaultPath,
            filter: ff,
            allowMultipleSelections: false
            );

        return result.Canceled ? null : result.Paths.ElementAt(0);
    }

    public static string? ShowSelectFolderDialog(string defaultPath)
    {
        var result = TinyDialogs.SelectFolderDialog(
            title: "Select Scene Folder",
            defaultPath: defaultPath);

        return result.Path;
    }
}