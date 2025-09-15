using System.ComponentModel;
using System.Numerics;
using System.Text;
using CSCanbulatEngine.InfoHolders;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

//Represents object in game world, with transform and mesh
public class GameObject
{
    public string Name { get; set; }
    public List<Component> Components { get; }
    public List<string> Tags { get; set; }
    public int ID { get; set; }

    public GameObject(Mesh mesh, string name = "GameObject")
    {
        Name = name;
        Components = new List<Component>();
        Tags = new List<string>();
        Tags.Add("GameObject");
        
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

        for (int i = 0; i <= Engine.currentScene.GameObjects.Count; i++)
        {
            if (FindGameObject(i) is null) ID = i;
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

    public int selectedTag = 0;
    public static void RenderGameObjectInspector()
    {
        if (Engine._selectedGameObject != null)
        {
            ImGui.Text($"Editing {Engine._selectedGameObject.gameObject.Name}");
            ImGui.Text($"ID: {Engine._selectedGameObject.gameObject.ID}");

            ImGui.Separator();
            
            ImGui.BeginChild("CustomTagList", new Vector2(0, 150), ImGuiChildFlags.Borders);
            
            GameObject selectedGameObject = Engine._selectedGameObject.gameObject;
            
            ImGui.Text("Tags for GameObject:");
            ImGui.SameLine();
            ImGui.ImageButton("RewriteName", (IntPtr)LoadIcons.icons["Rewrite.png"], new(25));

            for (int i = 0; i < selectedGameObject.Tags.Count; i++)
            {
                string tag = selectedGameObject.Tags[i];

                string selectableLabel = $"{tag}##{i}";

                if (ImGui.Selectable(selectableLabel, i == selectedGameObject.selectedTag, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    selectedGameObject.selectedTag = i;
                }

                float typeNameWidth = ImGui.CalcTextSize(selectedGameObject.Tags[i]).X;
                float columnWidth = ImGui.GetContentRegionAvail().X - 5;
            }
            ImGui.EndChild();
            
            ImGui.Separator();

            foreach (Component component in Engine._selectedGameObject.gameObject.Components)
            {
                if (ImGui.CollapsingHeader(component.name, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    component.RenderInspector();
                }
            }
        }
        
        
    }
#endif

    public static GameObject? FindGameObject(int id)
    {
        foreach (var gameObject in Engine.currentScene.GameObjects)
        {
            if (gameObject.ID == id) return gameObject;
        }

        return null;
    }

}