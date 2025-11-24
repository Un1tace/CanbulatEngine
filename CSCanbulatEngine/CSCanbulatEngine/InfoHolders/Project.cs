using System.Numerics;
using System.Runtime.InteropServices;
using CSCanbulatEngine.FileHandling;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using Silk.NET.Maths;

namespace CSCanbulatEngine.InfoHolders;

public class Project
{
    public string ProjectFolderPath { get; set; }
    
    public string ProjectName { get; set; }
    
    public string LastOpenedSceneName { get; set; }
    
    public string LastOpenedScenePath { get; set; }

    public string StartupSceneName { get; set; } = "ExampleScene.cbs";

    public Project(string ProjectName, string ProjectPath)
    {
        this.ProjectName = ProjectName;
        this.ProjectFolderPath = ProjectPath;
    }
    

}