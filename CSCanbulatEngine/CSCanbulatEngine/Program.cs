namespace CSCanbulatEngine;

class Program
{
    static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += HandleCrash;
        Engine engine = new Engine();

        engine.Run();
    }

    private static void HandleCrash(object sender, UnhandledExceptionEventArgs args)
    {
        Exception e = (Exception)args.ExceptionObject;

        try
        {
            // 1. Get the path to your app's private data folder. This requires no permissions.
            // On macOS, this is usually ~/.config or ~/Library/Application Support
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string logDirectory = Path.Combine(appDataPath, "YourAppName"); // Create a folder for your app

            // Ensure the directory exists
            Directory.CreateDirectory(logDirectory);

            // 2. Define the log file path inside this safe directory.
            string logFilePath = Path.Combine(logDirectory, "CrashLog.txt");

            // 3. Create the log content.
            string logContent =
                $"CRASH REPORT - {DateTime.Now}\n" +
                "------------------------------------------\n" +
                $"Error Type: {e.GetType().Name}\n" +
                $"Message: {e.Message}\n\n" +
                $"Stack Trace:\n{e.StackTrace}\n";

            // 4. Write the content to the log file.
            File.WriteAllText(logFilePath, logContent);
        }
        catch
        {
            // If even logging fails, there's nothing more we can do.
        }
    }
}