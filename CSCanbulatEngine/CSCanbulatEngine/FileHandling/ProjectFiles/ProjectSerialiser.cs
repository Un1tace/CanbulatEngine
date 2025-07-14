using System.Text;
using CSCanbulatEngine.InfoHolders;
using ImGuiNET;
using Newtonsoft.Json;

namespace CSCanbulatEngine.FileHandling;

public class ProjectSerialiser
{
    private static string projectName;
    private static string? projectFolder;
    
    //Project file structure
    //{Project Name Folder}
    //|_ {projectname}.cbp
    //|_ Assets
    //      |_ Images
    //      |_ GameObjects/Prefabs
    //      |_ Audio
    //      |_ Scenes
    // ! ! ! When building the game all ofthis will have to be moved to somewhere to be built and accessed ! ! !
    
    

    #if EDITOR
    public static void CreateProjectFiles()
    {
        projectFolder = FileDialogHelper.ShowSelectFolderDialog(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Select a folder to store your project");
        
        if (String.IsNullOrWhiteSpace(projectFolder)) return;
        
        string[] files = Directory.GetFiles(projectFolder, "*.cbp");
        if (files.Length == 0)
        {
            ShowNamePopUp();
        }

        Engine.projectFoundPopup = true;
    }

    private static void ShowNamePopUp()
    {
        //Shows popup for project name
        Array.Clear(Engine._nameBuffer, 0, Engine._nameBuffer.Length);
        byte[] currentNameBytes = Encoding.UTF8.GetBytes("");
        Array.Copy(currentNameBytes, Engine._nameBuffer, currentNameBytes.Length);
        Engine.createProjectPopup = true;
    }

    private static void CreateProjectFilesContinued()
    {
        if (String.IsNullOrWhiteSpace(projectName)) return;
        
        string? projectFile = FindProjectFile(projectFolder);

        if (!string.IsNullOrWhiteSpace(projectFile))
        {
            Engine.projectFoundPopup = true;
            return;
        }

        CreateFiles(projectName, projectFolder);
    }

    private static void CreateFiles(string projectName, string projectFolder)
    {
        string projectFile = Path.Combine(projectFolder, projectName);
        
        string assetFolderPath = Path.Combine(projectFolder, "Assets");
        Directory.CreateDirectory(assetFolderPath);
        Directory.CreateDirectory(Path.Combine(assetFolderPath, "Images"));
        Directory.CreateDirectory(Path.Combine(assetFolderPath, "Audio"));
        Directory.CreateDirectory(Path.Combine(assetFolderPath, "GameObjects"));
        Directory.CreateDirectory(Path.Combine(assetFolderPath, "Scenes"));
        
        SaveProjectFile(projectFolder, projectName);
        
        LoadProjectFile(projectFolder);
    }
#endif
    
    public static bool LoadProjectFile(string projectFolder)
    {
        string[] files = Directory.GetFiles(projectFolder);
        string? projectFileName = null;
        foreach (string file in files)
        {
            if (Path.GetExtension(file).ToLower() == ".cbp")
            {
                projectFileName = Path.GetFileName(file);
            }
        }

        if (String.IsNullOrWhiteSpace(projectFileName))
        {
            return false;
        }
        
        var projectJson = File.ReadAllText(Path.Combine(projectFolder, projectFileName));
        var projectData = JsonConvert.DeserializeObject<ProjectData.ProjectInfo>(projectJson);
        
        Engine.currentProject = new Project(projectData.ProjectName, projectFolder);
        Engine.currentProject.LastOpenedSceneName = projectData.LastOpenedScene;
        Engine.currentProject.LastOpenedScenePath = projectData.LastOpenedScenePath;

        Console.WriteLine($"Opened project file: {projectFileName}");
        return true;
    }

    public static void LoadProject()
    {
        projectFolder = null;
        do
        {
            projectFolder = FileDialogHelper.ShowSelectFolderDialog(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Select a folder to open/store your project");
        } while (String.IsNullOrWhiteSpace(projectFolder) || !Directory.Exists(projectFolder));
        
        string projectFile = FindProjectFile(projectFolder);
        
        LoadProjectFile(projectFolder);
    }

#if EDITOR
    //Gets assets folder and checks correct directories
    public static string GetAssetsFolder()
    {
        string projectPath = Engine.currentProject.ProjectFolderPath;

        if (!Directory.Exists(Path.Combine(projectPath, "Assets")))
        {
            string assetFolderPath = Path.Combine(projectPath, "Assets");
            Directory.CreateDirectory(assetFolderPath);
            Directory.CreateDirectory(Path.Combine(assetFolderPath, "Images"));
            Directory.CreateDirectory(Path.Combine(assetFolderPath, "Audio"));
            Directory.CreateDirectory(Path.Combine(assetFolderPath, "GameObjects"));
            Directory.CreateDirectory(Path.Combine(assetFolderPath, "Scenes"));
            return assetFolderPath;
        }
        
        string assetPath = Path.Combine(projectPath, "Assets");
        
        if (!Directory.Exists(Path.Combine(assetPath, "Images")))
        {
            Directory.CreateDirectory(Path.Combine(assetPath, "Images"));
        }

        if (!Directory.Exists(Path.Combine(assetPath, "Audio")))
        {
            Directory.CreateDirectory(Path.Combine(assetPath, "Audio"));
        }

        if (!Directory.Exists(Path.Combine(assetPath, "GameObjects")))
        {
            Directory.CreateDirectory(Path.Combine(assetPath, "GameObjects"));
        }

        if (!Directory.Exists(Path.Combine(assetPath, "Scenes")))
        {
            Directory.CreateDirectory(Path.Combine(assetPath, "Scenes"));
        }

        return assetPath;
    }
    //Saves the project file information
    public static void SaveProjectFile(string projectFolderPath, string projectName)
    {
        var projectData = new ProjectData.ProjectInfo();
        projectData.ProjectName = projectName;
        projectData.ProjectPath = projectFolderPath;
        projectData.LastOpenedScene = Engine.currentScene?.SceneName;
        projectData.LastOpenedScenePath = Engine.currentScene?.SceneFilePath;

        string projectJson = JsonConvert.SerializeObject(projectData);
        File.WriteAllText(Path.Combine(projectFolderPath, projectName + ".cbp"), projectJson);
        Console.WriteLine("Project File Saved To: " + Path.Combine(projectFolderPath, projectName + ".cbp"));
    }

    //Create or load when the engine starts up
    public static void CreateOrLoadProjectFile()
    {
        projectFolder = null;
        do
        {
            projectFolder = FileDialogHelper.ShowSelectFolderDialog(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Select a folder to open/store your project");
        } while (String.IsNullOrWhiteSpace(projectFolder) || !Directory.Exists(projectFolder));
        
        string projectFile = FindProjectFile(projectFolder);

        if (String.IsNullOrWhiteSpace(projectFile))
        {
            ShowNamePopUp();
            return;
        }
        
        LoadProjectFile(projectFolder);
    }
#endif
    
    public static string? FindProjectFile(string folder)
    {
        string[] files = Directory.GetFiles(folder);
        if (files.Length == 0) return null;

        foreach (string file in files)
        {
            if (file.EndsWith(".cbp")) return file;
        }

        return null;
    }

#if EDITOR
    public static void CreateProjectPopUp()
    {
        if (ImGui.BeginPopupModal("Name Project", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("Enter a name for the scene");
            ImGui.InputText("##NameInput", Engine._nameBuffer, (uint)Engine._nameBuffer.Length);

            if (ImGui.Button("OK"))
            {
                string newName = Encoding.UTF8.GetString(Engine._nameBuffer).TrimEnd('\0');
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    projectName = newName;
                    CreateProjectFilesContinued();
                }

                Engine.createProjectPopup = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                Engine.createProjectPopup = false;
                ImGui.CloseCurrentPopup();
                
            }
            ImGui.EndPopup();
        }
    }

    public static void ProjectAlreadyHerePopup()
    {
        if (ImGui.BeginPopupModal("Project Found", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("There is already a project in this folder");
            ImGui.Separator();

            if (ImGui.Button("Cancel"))
            {
                Engine.projectFoundPopup = false;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }
#endif
}