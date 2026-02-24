namespace CSCanbulatEngine.FileHandling.Game;

public record GameConfig
{
    public string StartupSceneName { get; set; } = "";
    public string GameName { get; set; } = "My Game";
}