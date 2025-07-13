using CSCanbulatEngine.GameObjectScripts;

namespace CSCanbulatEngine.InfoHolders;

public class Scene
{
    public string SceneFilePath { get; set; }
    
    public string SceneName { get; set; }

    public List<GameObject> GameObjects;

    public Scene(string sceneName)
    {
        this.SceneName = sceneName;
        GameObjects = new List<GameObject>();
    }
}