namespace CSCanbulatEngine.InfoHolders;

public class Project
{
    public string ProjectFolderPath { get; set; }
    
    public string ProjectName { get; set; }
    
    public string LastOpenedSceneName { get; set; }
    
    public string LastOpenedScenePath { get; set; }

    public Project(string ProjectName, string ProjectPath)
    {
        this.ProjectName = ProjectName;
        this.ProjectFolderPath = ProjectPath;
    }
}