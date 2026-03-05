using System.Text.Json.Serialization;
using CSCanbulatEngine.Circuits;
using CSCanbulatEngine.GameObjectScripts;
using CSCanbulatEngine.InfoHolders;
using CSCanbulatEngine.Mesh;
using Silk.NET.OpenGL;
using Newtonsoft.Json;

namespace CSCanbulatEngine.FileHandling;

/// <summary>
/// Class to load and save scenes into the game
/// </summary>
public class SceneSerialiser
{
    private readonly GL _gl;
    private readonly GameObjectScripts.Mesh _defaultMesh;

    public SceneSerialiser(GL gl, GameObjectScripts.Mesh defaultMesh)
    {
        _gl = gl;
        _defaultMesh = defaultMesh;
    }
#if EDITOR
    public static void SaveScene(string sceneName = "Example Scene")
    {
        string filePath = Path.Combine(ProjectSerialiser.GetAssetsFolder(), "Scenes");
        var sceneData = SceneDataFromCurrentScene(sceneName);

        string json = JsonConvert.SerializeObject(sceneData, Formatting.Indented);
        File.WriteAllText(Path.Combine(filePath, sceneName + ".cbs"), json);
        Engine.currentScene.SceneFilePath = Path.Combine(filePath, sceneName + ".cbs");
        EngineLog.Log($"Saved scene: {Path.Combine(filePath, sceneName + ".cbs")}");
        Engine.currentScene.SceneSavedOnce = true;
    }
    
    public static SceneData.SceneInfo SceneDataFromCurrentScene(string sceneName = "Example Scene")
    {
        string filePath = Path.Combine(ProjectSerialiser.GetAssetsFolder(), "Scenes");
        var sceneData = new SceneData.SceneInfo();
        
        sceneData.SceneFilePath = filePath;
        sceneData.SceneName = sceneName;
        sceneData.SceneSettings = SceneSettings.GetSceneSettings();
        sceneData.GameObjects = new List<SceneData.GameObjectData>();
        sceneData.Events = new List<SceneData.EventData>();

        foreach (var theEvent in EventManager.RegisteredEvents)
        {
            SceneData.EventData eventData = new()
            {
                Name = theEvent.EventName,
                canConfig = theEvent.CanConfig,
                canReceive = theEvent.CanReceive,
                canSend = theEvent.CanSend,
                eventValuesData = theEvent.baseValues
            };
            sceneData.Events.Add(eventData);
        }
        
        foreach (var obj in Engine.currentScene.GameObjects)
        {
            // List<SceneData.ComponentData> componentData = new();
            // foreach (var component in obj.Components)
            // {
            //     SceneData.ComponentData toAdd = new()
            //     {
            //         Name = component.name,
            //         Enabled = component.isEnabled,
            //         ComponentType = component.GetType().FullName,
            //         CustomProperties = component.GetCustomProperties()
            //     };
            //     componentData.Add(toAdd);
            // }
            // sceneData.GameObjects.Add(new SceneData.GameObjectData(){Name = obj.Name, ObjectID = obj.ID, Tags = obj.Tags, ParentObjectID = obj.ParentObject?.ID, ComponentData = componentData});
            sceneData.GameObjects.Add(GetGameObjectData(obj));
        }

        return sceneData;
    }

    /// <summary>
    /// Gets the GameObjectData from game object
    /// </summary>
    /// <param name="obj">GameObject</param>
    /// <returns>GameObjectData</returns>
    public static SceneData.GameObjectData GetGameObjectData(GameObject obj)
    {
        // Create data object for components
        List<SceneData.ComponentData> componentData = new();
        foreach (var component in obj.Components)
        {
            SceneData.ComponentData toAdd = new()
            {
                Name = component.name,
                Enabled = component.isEnabled,
                ComponentType = component.GetType().FullName,
                CustomProperties = component.GetCustomProperties()
            };
            componentData.Add(toAdd);
        }
        
        // Gets data from objects and returns it
        return new SceneData.GameObjectData()
        {
            Name = obj.Name, ObjectID = obj.ID, Tags = obj.Tags, ParentObjectID = obj.ParentObject?.ID,
            ComponentData = componentData, ObjectType = obj.objectType
        };
    }
    
#endif
    
    /// <summary>
    /// Sets a gameobject already made to data provided
    /// </summary>
    /// <param name="setToObj">Object to set data to</param>
    /// <param name="objData">Object Data</param>
    /// <param name="meshToUse">Mesh to use</param>
    public static void SetGameObjectData(GameObject setToObj, SceneData.GameObjectData objData, GameObjectScripts.Mesh meshToUse)
    {
        setToObj.Tags = objData.Tags ?? new List<string>();

        foreach (var componentData in objData.ComponentData)
        {
            var componentType = Type.GetType(componentData.ComponentType);
            if (componentType is null)
            {
                EngineLog.Log($"[SceneSerialiser] Couldn't find component of type {componentData.ComponentType}");
                continue;
            }

            Component newComponent;

            if (componentType == typeof(MeshRenderer))
            {
                newComponent = new MeshRenderer(meshToUse);
            }
            else
            {
                newComponent = (Component)Activator.CreateInstance(componentType);
            }
                
            setToObj.AddComponent(newComponent);
            newComponent.isEnabled = componentData.Enabled;
            newComponent.SetCustomProperties(componentData.CustomProperties);
        }
    }

    /// <summary>
    /// Creates game object from data provided
    /// </summary>
    /// <param name="objData">Game object data</param>
    /// <param name="useNextAvaliableID">Determines if old id is needed or a new one</param>
    /// <returns></returns>
    public static GameObject CreateGameObjectFromData(SceneData.GameObjectData objData, bool useNextAvaliableID = true)
    {
        GameObject obj;
        GameObjectScripts.Mesh meshToUse;
        // Gets mesh based on type
        if (objData.ObjectType == ObjectType.Circle)
        {
            meshToUse = ChunFactory.CreateCircle(32);
        }
        else if (objData.ObjectType == ObjectType.Triangle)
        {
            meshToUse = ChunFactory.CreateTriangle();
        }
        else meshToUse = ChunFactory.CreateQuad();
        
        // Determines type of constructor to use
        if (useNextAvaliableID)
        {
            obj = new GameObject(meshToUse, objData.ObjectType?? ObjectType.Quad, objData.Name, false);
        }
        else
        {
            obj = new GameObject(objData.Name, objData.ObjectID.Value, meshToUse, objData.ObjectType?? ObjectType.Quad);
        }
            
        // Sets game object data
        SetGameObjectData(obj, objData, meshToUse);

        return obj;
    }

    public static void LoadSceneFromString(string json)
    {
        var sceneData = JsonConvert.DeserializeObject<SceneData.SceneInfo>(json);
        
        //Reset managers
        VariableManager.Clear();
        EventManager.Clear();
        #if EDITOR
        Engine._selectedGameObject = null;
#endif
        while (Engine.currentScene.GameObjects.Any())
        {
            Engine.currentScene.GameObjects[0].DeleteObject();
        }

        Engine.currentScene = new Scene(sceneData.SceneName);
        Engine.currentScene.SceneName = sceneData.SceneName;
        Engine.currentScene.SceneFilePath = sceneData.SceneFilePath;
        Engine.currentScene.SceneSavedOnce = true;
        
        SceneSettings.SetSceneSettings(sceneData.SceneSettings);

        if (sceneData.Events != null)
        {
            foreach (var eventData in sceneData.Events)
            {
                var theEvent = new Event(eventData.Name);
                theEvent.CanConfig = eventData.canConfig;
                theEvent.CanReceive = eventData.canReceive;
                theEvent.CanSend = eventData.canSend;
                theEvent.baseValues = eventData.eventValuesData;
                theEvent.EventName = eventData.Name;

                EventManager.RegisterEvent(theEvent);
            }
        }
        
        foreach (var objData in sceneData.GameObjects)
        {
            CreateGameObjectFromData(objData, false);
        }

        foreach (var objData in sceneData.GameObjects)
        {
            if (objData.ParentObjectID is not null && objData.ObjectID is not null &&
                GameObject.FindGameObject(objData.ParentObjectID.Value) is not null && GameObject.FindGameObject(objData.ObjectID.Value) is not null)
            {
                var thisObj = GameObject.FindGameObject(objData.ObjectID.Value);
                GameObject.FindGameObject(objData.ParentObjectID.Value).MakeParentOfObject(thisObj);
            }
        }
        
        EngineLog.Log("[SceneSerialiser] Loaded scene: " + sceneData.SceneName);
    }
    
    public void LoadScene(string filePath)
    {
        string json = File.ReadAllText(filePath);

        EngineLog.Log($"[SceneSerialiser] Loading scene {filePath}");
        
        LoadSceneFromString(json);
    }
}