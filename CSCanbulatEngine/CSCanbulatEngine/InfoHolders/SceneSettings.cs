using System.Drawing;
using System.Globalization;
using System.Numerics;
using CSCanbulatEngine.FileHandling;
using CSCanbulatEngine.GameObjectScripts;
using ImGuiNET;

namespace CSCanbulatEngine;

/// <summary>
/// Holds information and allows the change of settings over the entire project
/// </summary>
public static class SceneSettings
{
    public static bool isOpen = false;
    public static Vector2 Gravity = new Vector2(0, -9.81f);
    private static Vector4 colourSet = Engine.BackgroundColour.colour;

    /// <summary>
    /// Show project settings window
    /// </summary>
    public static void SceneSettingsWindow()
    {
         ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
         ImGui.SetNextWindowSize(new Vector2(400, 500), ImGuiCond.Appearing);

         ImGui.Begin("Scene Settings", ref isOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
         
         ImGui.Text("Colour");
         ImGui.SameLine();
         if (ImGui.ColorEdit4("Colour", ref colourSet))
         {
             Engine.BackgroundColour.colour = colourSet;
         }
         
         ImGui.Text("Gravity");
         ImGui.SameLine();
         ImGui.DragFloat2("Gravity", ref Gravity);

         ImGui.BeginDisabled(Engine.currentProject.ProjectFolderPath == null && Engine.currentProject.ProjectName == null);
         if (ImGui.Button("Save"))
         {
             SceneSerialiser.SaveScene(Engine.currentScene.SceneName);
         }
         ImGui.EndDisabled();
         
         if (ImGui.Button("Reset To Defaults"))
         {
             ResetToDefaults();
         }

         if (ImGui.Button("Reset Camera Position and Zoom"))
         {
             Engine.ResetCameraPosAndZoom();
         }
         
         ImGui.End();
    }

    /// <summary>
    /// Get Scene settings in Dictionary<string, string> format
    /// </summary>
    /// <returns>Scene Settings</returns>
    public static Dictionary<string, string> GetSceneSettings()
    {
        return new Dictionary<string, string>()
        {
            { "Gravity.X", Gravity.X.ToString(CultureInfo.InvariantCulture) },
            { "Gravity.Y", Gravity.Y.ToString(CultureInfo.InvariantCulture) },
            {"BackgroundR", colourSet.X.ToString(CultureInfo.InvariantCulture) },
            {"BackgroundG", colourSet.Y.ToString(CultureInfo.InvariantCulture) },
            {"BackgroundB", colourSet.Z.ToString(CultureInfo.InvariantCulture) },
            {"BackgroundA", colourSet.W.ToString(CultureInfo.InvariantCulture) }
        };
    }

    /// <summary>
    /// Sets scene settings from information to current
    /// </summary>
    /// <param name="settings">Scene settings information</param>
    public static void SetSceneSettings(Dictionary<string, string>? settings)
    {
        if (settings == null || !settings.Any())
        {
            ResetToDefaults();
            return;
        }
        Gravity.X = float.Parse(settings["Gravity.X"], CultureInfo.InvariantCulture);
        Gravity.Y = float.Parse(settings["Gravity.Y"], CultureInfo.InvariantCulture);

        if (settings.TryGetValue("Gravity.X", out var x))
        {
            if (float.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var xf))
            {
                Gravity.X = xf;
            }
        }

        if (settings.TryGetValue("Gravity.Y", out var y))
        {
            if (float.TryParse(y, NumberStyles.Float, CultureInfo.InvariantCulture, out var yf))
            {
                Gravity.Y = yf;
            }
        }

        if (settings.TryGetValue("BackgroundR", out var backgroundR))
        {
            if (float.TryParse(backgroundR, NumberStyles.Float, CultureInfo.InvariantCulture, out float r))
            {
                colourSet.X = r;
                Engine.BackgroundColour.r = r;
            }
        }

        if (settings.TryGetValue("BackgroundG", out var backgroundG))
        {
            if (float.TryParse(backgroundG, NumberStyles.Float, CultureInfo.InvariantCulture, out float g))
            {
                colourSet.Y = g;
                Engine.BackgroundColour.g = g;
            }
        }

        if (settings.TryGetValue("BackgroundB", out var backgroundB))
        {
            if (float.TryParse(backgroundB, NumberStyles.Float, CultureInfo.InvariantCulture, out float b))
            {
                colourSet.Z = b;
                Engine.BackgroundColour.b = b;
            }
        }

        if (settings.TryGetValue("BackgroundA", out var backgroundA))
        {
            if (float.TryParse(backgroundA, NumberStyles.Float, CultureInfo.InvariantCulture, out float a))
            {
                colourSet.W = a;
                Engine.BackgroundColour.a = a;
            }
        }
    }

    /// <summary>
    /// Reset project settings to default project settings.
    /// </summary>
    public static void ResetToDefaults()
    {
        Gravity = new Vector2(0, -9.81f);
        Engine.BackgroundColour = new Colour()
        {
            r = Color.CornflowerBlue.R / 255f,
            g = Color.CornflowerBlue.G / 255f,
            b = Color.CornflowerBlue.B / 255f,
            a = Color.CornflowerBlue.A / 255f
        };
        colourSet = Engine.BackgroundColour.colour;
    }
}