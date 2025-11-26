namespace CSCanbulatEngine;

class Program
{
    static void Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        AppDomain.CurrentDomain.UnhandledException += HandleCrash;
        EngineLog.OnStart();
        Console.SetOut(new ConsoleOutputCapturer());
        Engine engine = new Engine();

        engine.Run();
    }

    private static void HandleCrash(object sender, UnhandledExceptionEventArgs args)
    {
        Exception e = (Exception)args.ExceptionObject;

        try
        {
            string logContent =
                $"CRASH REPORT - {DateTime.Now}\n" +
                "------------------------------------------\n" +
                $"Error Type: {e.GetType().Name}\n" +
                $"Message: {e.Message}\n\n" +
                $"Stack Trace:\n{e.StackTrace}\n";
            
            EngineLog.CrashLog(logContent);
        }
        catch
        {
        }
    }
}