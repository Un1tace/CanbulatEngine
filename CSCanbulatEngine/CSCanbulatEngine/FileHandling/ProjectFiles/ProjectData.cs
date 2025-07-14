namespace CSCanbulatEngine.FileHandling;

public class ProjectData
{
    public class ProjectInfo
    {
        public string ProjectPath { get; set; }
        public string ProjectName { get; set; }
        
        public string? LastOpenedScene { get; set; }
        public string? LastOpenedScenePath { get; set; }
    }
}