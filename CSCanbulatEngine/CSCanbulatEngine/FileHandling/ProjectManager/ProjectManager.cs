using System.Numerics;
using System.Runtime.InteropServices;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using Silk.NET.Maths;

namespace CSCanbulatEngine.FileHandling.ProjectManager;

public static class ProjectManager
{
    private static readonly string[] _ignoredFileTypes = new[] { ".DS_Store" };
    
        #if EDITOR

    public static string selectedDir = "";
    public static void RenderDirectories()
    {
        foreach (string dir in Directory.GetDirectories(ProjectSerialiser.GetAssetsFolder()))
        {
            RenderDirNode(dir);
        }
    }
    
    // Project file manager
    // List on side
    public static void RenderDirNode(string dir)
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick;

        if (selectedDir == dir)
        {  
            flags |= ImGuiTreeNodeFlags.Selected;
        }

        if (Directory.GetDirectories(dir).Length == 0)
        {
            flags = ImGuiTreeNodeFlags.Leaf;
        }

        bool nodeOpen = ImGui.TreeNodeEx(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)? dir.Split('\\').Last():dir.Split("/").Last(), flags);
        
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            selectedDir = Path.Combine(ProjectSerialiser.GetAssetsFolder(), FileHandling.GetNameOfFile(dir));
            Console.WriteLine(selectedDir);
        }

        if (nodeOpen)
        {
            foreach (string childDir in Directory.GetDirectories(dir))
            {
                RenderDirNode(childDir);
            }

            ImGui.TreePop();
        }
    }

    public static bool DrawFileIcon(IntPtr textureId, string label, Vector2 size)
    {
        ImGui.PushID(label);
        
        ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);

        bool clicked = ImGui.ImageButton(label, textureId, size);
        
        ImGui.PopStyleColor();

        float textWidth = ImGui.CalcTextSize(label).X;
        
        float iconWidth = ImGui.GetItemRectSize().X;
        
        float textPadding = (iconWidth - textWidth) * 0.5f;
        
        if (textPadding > 0)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + textPadding);
        }
            
        ImGui.Text(label);

        ImGui.PopID();

        return clicked;
    }

    public static void RenderProjectManagerIcons()
    {
        if (String.IsNullOrWhiteSpace(selectedDir)) selectedDir = ProjectSerialiser.GetAssetsFolder();
        float iconSize = 64;
        float cellPadding = 16;
        float totalCellWidth = iconSize + cellPadding;
        
        float avaliableWidth = ImGui.GetContentRegionAvail().X;
        
        int columnCount = (int)(avaliableWidth / totalCellWidth);

        if (columnCount < 1)
        {
            columnCount = 1;
        }
        
        ImGui.Columns(columnCount, "FileGrid", false);

        string[] files = Directory.GetFiles(selectedDir);
        string[] dirs = Directory.GetDirectories(selectedDir);
        
        List<string> listedItems = new List<string>();
        listedItems.AddRange(files);
        listedItems.AddRange(dirs);

        foreach (var name in listedItems)
        {
            if (_ignoredFileTypes.Contains(Path.GetExtension(name)))
            {
                continue;
            }
            
            FileAttributes attr = File.GetAttributes(name);

            bool isDir = attr.HasFlag(FileAttributes.Directory);
            
            uint iconID = isDir ? LoadIcons.icons["Folder.png"] : LoadIcons.icons["Page.png"];
            
            Vector2D<int> actualSize = isDir ? LoadIcons.iconSizes["Folder.png"] : LoadIcons.iconSizes["Page.png"];

            bool isHeightBigger = actualSize.Y > actualSize.X;

            Vector2 size = isHeightBigger ? new Vector2(iconSize * (int)(actualSize.X/actualSize.Y),iconSize) : new Vector2(iconSize, iconSize * (int)(actualSize.Y / actualSize.X));
            
            if (DrawFileIcon((IntPtr)iconID, FileHandling.GetNameOfFile(name), size))
            {
                Console.WriteLine($"Clicked on {name}");
                if (isDir)
                {
                    selectedDir = name;
                }
            }

            ImGui.NextColumn();
        }
        
        ImGui.Columns(1);
    }
    #endif
}