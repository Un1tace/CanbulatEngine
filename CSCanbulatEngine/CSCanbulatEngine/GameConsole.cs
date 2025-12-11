using System.Numerics;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;

namespace CSCanbulatEngine;

public static class GameConsole
{
    public static List<LogInfo> logs = new List<LogInfo>();

    public static void Log(string log, LogType type = LogType.Normal)
    {
        logs.Add(new LogInfo(log, type));
        
        #if EDITOR
        Engine._forceSetConsoleTab = true;
        #endif
    }

    /// <summary>
    /// Clears all logs.
    /// </summary>
    public static void ClearLog()
    {
        logs.Clear();
    }

    /// <summary>
    /// Renders Console In ImGui
    /// </summary>
    public static void RenderConsole()
    {
        ImGui.Text("Console Logs");
        ImGui.Separator();
        ImGui.BeginChild("Logs");

        var uv0 = new Vector2(1, 0);
        var uv1 = new Vector2(0, 1);
        for (int i = logs.Count - 1; i >= 0; i--)
        {
            LogInfo log = logs[i];
            ImGui.BeginGroup();
            {
                Vector4 tintColor = new Vector4(1, 1, 1, 1);
                
                IntPtr icon = (IntPtr)LoadIcons.icons["Log.png"];
                switch (log.logType)
                {
                    case LogType.Normal:
                        icon = (IntPtr)LoadIcons.icons["Log.png"];
                        break;
                    case LogType.Warning:
                        icon = (IntPtr)LoadIcons.icons["Warning.png"];
                        tintColor = new Vector4(1, 1, 0, 1);
                        break;
                    case LogType.Error:
                        icon = (IntPtr)LoadIcons.icons["Error.png"];
                        tintColor = new Vector4(1, 0, 0, 1);
                        break;
                }
                ImGui.Image(icon, new Vector2(50, 50), uv0, uv1, tintColor);
                ImGui.SameLine();
                ImGui.Text($"[{log.time:HH:mm:ss.fff}]" + log.log);
            }
            ImGui.EndGroup();
        }
        ImGui.EndChild();
    }
}

public class LogInfo
{
    public string log;
    public DateTime time;
    public LogType logType;

    public LogInfo(string log, LogType logType)
    {
        this.log = log;
        this.logType = logType;
        time = DateTime.Now;
    }
}

public enum LogType
{
    Normal, Warning, Error
}