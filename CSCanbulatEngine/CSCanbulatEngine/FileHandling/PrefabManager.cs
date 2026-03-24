using CSCanbulatEngine.GameObjectScripts;
using Newtonsoft.Json;

namespace CSCanbulatEngine.FileHandling;

/// <summary>
/// Handles loading and saving prefabs
/// </summary>
public class PrefabManager
{
    #if EDITOR
    // Saving Prefabs
    /// <summary>
    /// Saves a gameobject to a file
    /// </summary>
    /// <param name="rootObject">The object to save</param>
    /// <param name="directory">Directory where to save prefab</param>
    public static void SavePrefab(GameObject rootObject)
    {
        string filePath = Path.Combine(ProjectSerialiser.GetAssetsFolder(), Path.Combine("GameObjects", $"{rootObject.Name}.cfab"));
        SceneData.GameObjectData prefabData = SerialiseObjectRecursive(rootObject);
        
        string json = JsonConvert.SerializeObject(prefabData, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
    
    /// <summary>
    /// Turns game object into gameobject data recursively 
    /// </summary>
    /// <param name="obj">obj data</param>
    /// <returns></returns>
    private static SceneData.GameObjectData SerialiseObjectRecursive(GameObject obj)
    {
        SceneData.GameObjectData data = SceneSerialiser.GetGameObjectData(obj);

        data.Children = new List<SceneData.GameObjectData>();
        foreach (GameObject child in obj.ChildObjects)
        {
            data.Children.Add(SerialiseObjectRecursive(child));
        }
        return data;
    }
#endif

    
    // Loading prefabs
    /// <summary>
    /// Loads prefab from file data
    /// </summary>
    /// <param name="filePath">Loads from filepath to file</param>
    /// <returns>GameObject</returns>
    public static GameObject LoadPrefab(string filePath)
    {

        var newPath = filePath;
        if (!filePath.StartsWith("GameObjects/"))
        {
            string name = Path.GetFileName(filePath);
            
            newPath = "GameObjects/" + name;
        }
        
        newPath = Path.Combine(ProjectSerialiser.GetAssetsFolder(), newPath);
        
        if (!File.Exists(newPath))
        {
            GameConsole.Log("[Prefab Manager] File not found");
            return null;
        }

        if (!newPath.EndsWith(".cfab"))
        {
            GameConsole.Log("[Prefab Manager] File doesnt end with cfab");
            return null;
        }
        
        string json = File.ReadAllText(newPath);
        SceneData.GameObjectData data = JsonConvert.DeserializeObject<SceneData.GameObjectData>(json);

        return DeserialiseObjectRecursive(data);
    }
    
    /// <summary>
    /// Deserialises the game object data and turns it into a gameobject
    /// </summary>
    /// <param name="data">GameObjectData</param>
    /// <returns>GameObject</returns>
    private static GameObject DeserialiseObjectRecursive(SceneData.GameObjectData data)
    {
        GameObject newObj = SceneSerialiser.CreateGameObjectFromData(data, true);
        newObj.Name = data.Name + " (Clone)";

        if (data.Children != null || data.Children.Count > 0)
        {
            foreach (SceneData.GameObjectData childData in data.Children)
            {
                GameObject childObj = DeserialiseObjectRecursive(childData);
                childObj.MakeChildOfObject(newObj);
            }
        }

        return newObj;
    }
}

public class PrefabReference()
{
    public string FilePath { get; set; }
}