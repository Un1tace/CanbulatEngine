using System.ComponentModel;
using System.Text;
using CSCanbulatEngine.InfoHolders;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

//Represents object in game world, with transform and mesh
public class GameObject
{
    public string Name { get; set; }
    public List<Component> Components { get; }
    
    public List<string> Tags { get; set; }

    public GameObject(Mesh mesh, string name = "GameObject")
    {
        Name = name;
        Components = new List<Component>();
        Tags = new List<string>();
        
        //Add Core Components
        AddComponent(new Transform());
        AddComponent(new MeshRenderer(mesh));
        
        //
        int nameCheck = 0;
        string baseName = name;
        while (Engine.currentScene.GameObjects.Any(go => go.Name == Name))
        {
            nameCheck++;
            Name = $"{baseName}{nameCheck}";
        }
        
        
        if (!Engine.currentScene.GameObjects.Contains(this))
        {
            Engine.currentScene.GameObjects.Add(this);
        }
#if EDITOR
        Engine._selectedGameObject = new (this);
#endif
    }

    public T? GetComponent<T>() where T : Component
    {
        foreach (var component in Components)
        {
            if (component is T typedComponent)
            {
                return typedComponent;
            }
        }

        return null;
    }
    
    public int GetComponentIndex<T>() where T : Component
    {
        for (int i = 0; i < Components.Count; i++)
        {
            if (Components[i] is T typedComponent)
            {
                return i;
            }
        }

        return -1;
    }

    public void AddComponent(Component component) {
        
        Components.Add(component);
        component.AttachedGameObject = this;
    }
    
    public void RemoveComponent(Component component)
    {
        if (component.canBeRemoved)
        {
            component.DestroyComponent();
            Components.Remove(component);
        }

        Console.WriteLine($"Component ({component.name}) cannot be removed");
    }
    

    public void DeleteObject()
    {
        while (Components.Count > 0)
        {
            Components[0].DestroyComponent();
            Components.RemoveAt(0);
        }

        Engine.currentScene.GameObjects.Remove(this);
#if EDITOR
        if (Engine._selectedGameObject.gameObject == this)
        {
            Engine._selectedGameObject = null;
        }
#endif
    }

#if EDITOR
    public void RenderObjectOptionBar(string superKey)
    {
        if (ImGui.BeginMenu(Name))
        {
            

            if (ImGui.MenuItem("Remove", superKey + "+Back"))
            {
                DeleteObject();
            }
            if (ImGui.MenuItem("Rename", superKey + "+2"))
            {
                Array.Clear(Engine._nameBuffer, 0, Engine._nameBuffer.Length);
                if (Engine._selectedGameObject != null)
                {
                    byte[] currentNameBytes = Encoding.UTF8.GetBytes(Engine._selectedGameObject.gameObject.Name);
                    Array.Copy(currentNameBytes, Engine._nameBuffer, currentNameBytes.Length);
                    Engine.renamePopupOpen = true;
                }
                // ImGui.OpenPopup("Rename Object");
                
            }
            ImGui.EndMenu();
        }
    }
#endif

}