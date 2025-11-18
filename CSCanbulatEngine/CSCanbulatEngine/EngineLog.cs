using System.Text;
using Microsoft.IdentityModel.Logging;

namespace CSCanbulatEngine;

public static class EngineLog
{
    public static string LogFileName = "CanbulatEngineLog.txt";
    public static string LogFileFullPath;

    public static Queue<EngineLogHolder> LogQueue = new();
    private static bool fileExists = false;

    private static TextWriter _originalConsoleOut;
    
    public static void OnStart()
    {
        _originalConsoleOut = Console.Out;
        
        LogFileFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CanbulatEngine", "Logs");
        if (!Directory.Exists(Path.Combine(LogFileFullPath)))
        {
            Directory.CreateDirectory(Path.Combine(LogFileFullPath));
        }
        
        if (!File.Exists(Path.Combine(LogFileFullPath, LogFileName)))
        {
            File.Create(Path.Combine(LogFileFullPath, LogFileName)).Dispose();
            fileExists = true;
            EngineLog.Log($"Created log file at {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CanbulatEngine", "Logs", "CanbulatEngineLog.txt")}");
        }
        else
        {
            fileExists = true;
            EngineLog.Log($"Found log file at {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CanbulatEngine", "Logs", "CanbulatEngineLog.txt")}");
        }
    }

    public static void Log(string message)
    {
        LogInternal(message, true);
    }

    public static void LogFromSystem(string message)
    {
        EngineLogHolder newLog = new EngineLogHolder(message);
        LogQueue.Enqueue(newLog);

        if (fileExists)
        {
            string toPrint = $"[SYSTEM] [{newLog.logTime:dd/MM/yyyy HH:mm:ss:ff}] {newLog.log}";
            LogInternal(toPrint, true);
        }
    }

    private static void LogInternal(string message, bool printToConsole = true)
    {
        EngineLogHolder newLog = new EngineLogHolder(message);
        LogQueue.Enqueue(newLog);
        PrintLogs();
    }
    
    public static void PrintLogs()
    {
        if (fileExists)
        {
            while (LogQueue.Count > 0)
            {
                var nextLog = LogQueue.Dequeue();
                string toPrint = $"[{nextLog.logTime:dd/MM/yyyy HH:mm:ss:ff}] {nextLog.log}";

                if (nextLog.printToConsole && _originalConsoleOut != null)
                {
                    _originalConsoleOut.WriteLine(toPrint);
                }

                File.AppendAllText(Path.Combine(LogFileFullPath, LogFileName), toPrint + "\n");
            }
        }
    }

    public static void CrashLog(string log)
    {
        if (String.IsNullOrWhiteSpace(LogFileFullPath))
        {
            File.WriteAllText(Path.Combine(LogFileFullPath, "Crash.txt"), log);
        }
    }
}

public class EngineLogHolder
{
    public string log;
    public DateTime logTime;
    public bool printToConsole;

    public EngineLogHolder(string log, bool printToConsole = true)
    {
        this.log = log;
        logTime = DateTime.Now;
        this.printToConsole = printToConsole;
    }
}

public class ConsoleOutputCapturer : TextWriter
{
    private TextWriter _originalOut;

    public ConsoleOutputCapturer()
    {
        _originalOut = Console.Out;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void WriteLine(string? value)
    {
        if (value != null)
        {
            _originalOut.WriteLine(value);

            EngineLog.LogFromSystem(value);
        }
    }
}