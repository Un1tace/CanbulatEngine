namespace CSCanbulatEngine.FileHandling;

public class ProjectData
{
    public class ProjectInfo
    {
        public string ProjectPath { get; set; }
        public string ProjectName { get; set; }
        
        public string? LastOpenedScene { get; set; }
        public string? LastOpenedScenePath { get; set; }
        
        public string StartupSceneName { get; set; } = "ExampleScene";
        public Dictionary<string, string>? ProjectSettings { get; set; } = new Dictionary<string, string>();
    }
}