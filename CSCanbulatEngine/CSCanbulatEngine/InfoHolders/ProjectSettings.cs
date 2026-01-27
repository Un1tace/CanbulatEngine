using System.Globalization;
using System.Numerics;
using CSCanbulatEngine.FileHandling;
using ImGuiNET;

namespace CSCanbulatEngine;

/// <summary>
/// Holds information and allows the change of settings over the entire project
/// </summary>
public static class ProjectSettings
{
    public static bool isOpen = false;
    public static Vector2 Gravity = new Vector2(0, -9.81f);

    /// <summary>
    /// Show project settings window
    /// </summary>
    public static void ProjectSettingsWindow()
    {
         ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
         ImGui.SetNextWindowSize(new Vector2(400, 500), ImGuiCond.Appearing);

         ImGui.Begin("Project Settings", ref isOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
         
         ImGui.Text("Gravity");
         ImGui.SameLine();
         ImGui.DragFloat2("Gravity", ref Gravity);

         ImGui.BeginDisabled(Engine.currentProject.ProjectFolderPath == null && Engine.currentProject.ProjectName == null);
         if (ImGui.Button("Save"))
         {
             ProjectSerialiser.SaveProjectFile(Engine.currentProject.ProjectFolderPath, Engine.currentProject.ProjectName);
         }
         ImGui.EndDisabled();
         
         if (ImGui.Button("Reset To Defaults"))
         {
             ResetToDefaults();
         }
         
         ImGui.End();
    }

    /// <summary>
    /// Get project settings in Dictionary<string, string> format
    /// </summary>
    /// <returns>Project Settings</returns>
    public static Dictionary<string, string> GetProjectSettings()
    {
        return new Dictionary<string, string>()
        {
            { "Gravity.X", Gravity.X.ToString(CultureInfo.InvariantCulture) },
            { "Gravity.Y", Gravity.Y.ToString(CultureInfo.InvariantCulture) }
        };
    }

    /// <summary>
    /// Sets project settings from information to current
    /// </summary>
    /// <param name="settings">Project settings information</param>
    public static void SetProjectSettings(Dictionary<string, string>? settings)
    {
        if (settings == null || !settings.Any())
        {
            ResetToDefaults();
            return;
        }
        Gravity.X = float.Parse(settings["Gravity.X"], CultureInfo.InvariantCulture);
        Gravity.Y = float.Parse(settings["Gravity.Y"], CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Reset project settings to default project settings.
    /// </summary>
    public static void ResetToDefaults()
    {
        Gravity = new Vector2(0, -9.81f);
    }
}