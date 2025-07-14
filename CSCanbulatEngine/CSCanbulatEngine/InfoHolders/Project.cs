using System.Runtime.InteropServices;
using CSCanbulatEngine.FileHandling;
using ImGuiNET;

namespace CSCanbulatEngine.InfoHolders;

public class Project
{
    public string ProjectFolderPath { get; set; }
    
    public string ProjectName { get; set; }
    
    public string LastOpenedSceneName { get; set; }
    
    public string LastOpenedScenePath { get; set; }

    public Project(string ProjectName, string ProjectPath)
    {
        this.ProjectName = ProjectName;
        this.ProjectFolderPath = ProjectPath;
    }

    private static string selectedDir = "";
    public static void RenderDirectories()
    {
        foreach (string dir in Directory.GetDirectories(ProjectSerialiser.GetAssetsFolder()))
        {
            RenderDirNode(dir);
        }
    }

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
            selectedDir = dir;
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
}