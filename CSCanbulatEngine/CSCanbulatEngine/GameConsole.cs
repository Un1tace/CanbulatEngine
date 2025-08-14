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
    }

    public static void ClearLog()
    {
        logs.Clear();
    }

    public static void RenderConsole()
    {
        ImGui.Text("Console Logs");
        ImGui.Separator();
        ImGui.BeginChild("Logs");
        foreach (LogInfo log in logs)
        {
            ImGui.BeginGroup();
            {
                IntPtr icon = (IntPtr)LoadIcons.icons["Log.png"];
                switch (log.logType)
                {
                    case LogType.Normal:
                        icon = (IntPtr)LoadIcons.icons["Log.png"];
                        break;
                    case LogType.Warning:
                        icon = (IntPtr)LoadIcons.icons["Warning.png"];
                        break;
                    case LogType.Error:
                        icon = (IntPtr)LoadIcons.icons["Error.png"];
                        break;
                }
                ImGui.Image(icon, new Vector2(50, 50));
                ImGui.SameLine();
                ImGui.Text(log.log);
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