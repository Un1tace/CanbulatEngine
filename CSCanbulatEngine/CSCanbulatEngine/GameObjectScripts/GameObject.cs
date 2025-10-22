using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
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
    public GameObject? ParentObject { get; set; }
    public List<GameObject> ChildObjects { get; set; }

    public GameObject(Mesh mesh, string name = "GameObject")
    {
        Name = name;
        Components = new List<Component>();
        Tags = new List<string>();
        Tags.Add("GameObject");
        
        ChildObjects = new List<GameObject>();
        
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

    // Children/Parent handling
    public void MakeParentOfObject(GameObject childObject)
    {
        if (this != childObject && childObject != ParentObject)
        {
            if (childObject.ParentObject != null) childObject.RemoveParentObject();
            Vector2 childObjPosition = childObject.GetComponent<Transform>().WorldPosition;
            ChildObjects.Add(childObject);
            childObject.ParentObject = this;
            childObject.GetComponent<Transform>().WorldPosition = childObjPosition;
        }
    }

    public void MakeChildOfObject(GameObject parentObject)
    {
        if (!ChildObjects.Contains(parentObject) && this != parentObject)
        {
            if (ParentObject != null) RemoveParentObject();
            Vector2 childObjPosition = this.GetComponent<Transform>().WorldPosition;
            parentObject.ChildObjects.Add(this);
            ParentObject = parentObject;
            this.GetComponent<Transform>().WorldPosition = childObjPosition;
        }
    }

    public void RemoveChildObject(GameObject childObject)
    {
        if (ChildObjects.Contains(childObject))
        {
            Vector2 childObjPosition = childObject.GetComponent<Transform>().WorldPosition;
            ChildObjects.Remove(childObject);
            childObject.ParentObject = null;
            childObject.GetComponent<Transform>().WorldPosition = childObjPosition;
        }
    }

    public void RemoveParentObject()
    {
        if (ParentObject != null)
        {
            Vector2 childObjPosition = this.GetComponent<Transform>().WorldPosition;
            ParentObject.ChildObjects.Remove(this);
            ParentObject = null;
            this.GetComponent<Transform>().WorldPosition = childObjPosition;
        }
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

    public bool HasComponent<T>() where T : Component
    {
        return Components.Any(component => component is T);
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

        if (ParentObject != null)
        {
            RemoveParentObject();
        }

        if (ChildObjects.Count > 0)
        {
            while (ChildObjects.Count > 0)
            {
                RemoveChildObject(ChildObjects[0]);
            }
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
    private static byte[] _nameBuffer;
    private static byte[] _tagNameBuffer;
    private static bool makingNewTag = false;
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
            if (ImGui.ImageButton("RewriteName", (IntPtr)LoadIcons.icons["Rewrite.png"], new(25)))
            {
                if (Engine._selectedGameObject.gameObject.Tags.Count > 0)
                {
                    makingNewTag = false;
                    _tagNameBuffer = new byte[128];
                    UTF8Encoding.UTF8.GetBytes(
                        Engine._selectedGameObject.gameObject.Tags[Engine._selectedGameObject.gameObject.selectedTag]).CopyTo(_tagNameBuffer, 0);
                    ImGui.OpenPopup("NameNewTag");
                }
            }
            ImGui.SameLine();
            if (ImGui.ImageButton("AddTag", (IntPtr)LoadIcons.icons["Plus.png"], new(25)))
            {
                _tagNameBuffer = new byte[128];
                makingNewTag = true;
                ImGui.OpenPopup("NameNewTag");
            }
            ImGui.SameLine();
            if (ImGui.ImageButton("RemoveTag", (IntPtr)LoadIcons.icons["Trash.png"], new(25)))
            {
                Engine._selectedGameObject.gameObject.RemoveTag();
            }

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
            
            if (ImGui.BeginPopup("NameNewTag"))
            {
                ImGui.Text(makingNewTag ?  "New tag name" : "Configure tag name");
                ImGui.Separator();
                
                ImGui.InputText("##TagNameInput", _tagNameBuffer, (uint)_tagNameBuffer.Length);

                ImGui.SameLine();

                if (ImGui.Button("Set"))
                {
                    string newName = Encoding.UTF8.GetString(_tagNameBuffer).TrimEnd('\0');
                    if (!string.IsNullOrEmpty(newName))
                    {
                        if (makingNewTag)
                        {
                            Engine._selectedGameObject.gameObject.AddTag(newName);
                        }
                        else
                        {
                            Engine._selectedGameObject.gameObject.Tags[Engine._selectedGameObject.gameObject.selectedTag] = newName;
                        }
                    }
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
            
            ImGui.EndChild();
            
            ImGui.Separator();

            ComponentInstruction nextInstruction = new();
            
            foreach (Component component in Engine._selectedGameObject.gameObject.Components)
            {
                if (ImGui.CollapsingHeader(component.name, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if (component.canBeDisabled)
                    {
                        if (ImGui.Checkbox("Enabled", ref component._isEnabled))
                        {
                            
                        }
                    }
                    if (component.canBeRemoved)
                    {
                        if (ImGui.Button("Remove Component", new (ImGui.GetContentRegionAvail().X, ImGui.CalcTextSize("Add Component").Y)))
                        {
                            // Engine._selectedGameObject.gameObject.RemoveComponent(component);
                            nextInstruction._component = component;
                            nextInstruction._instructionType = InstructionType.Remove;
                            continue;
                        }
                    }
                    component.RenderInspector();
                }
            }

            if (nextInstruction._instructionType == InstructionType.Remove && nextInstruction._component != null)
            {
                Engine._selectedGameObject.gameObject.RemoveComponent(nextInstruction._component);
            }

            ImGui.Separator();
            
            Vector2 AddComponentSize = ImGui.GetContentRegionAvail();
            AddComponentSize.Y = ImGui.CalcTextSize("Add Component").Y * 2;

            if (ImGui.Button("Add Component", AddComponentSize))
            {
                ImGui.OpenPopup("AddComponentMenu");
            }
            
            if (ImGui.BeginPopup("AddComponentMenu"))
            {
                ImGui.Text("Available Components: ");
                ImGui.Separator();
                    
                if (!Engine._selectedGameObject.gameObject.HasComponent<MeshRenderer>()) 
                { 
                    if (ImGui.MenuItem("MeshRenderer")) 
                    {
                        var meshRenderer = new MeshRenderer(Engine._squareMesh);
                        Engine._selectedGameObject.gameObject.AddComponent(meshRenderer); 
                    }
                }
                    
                ImGui.EndPopup();
            }
        }
        
    }

    public void AddTag(string tag)
    {
        Engine._selectedGameObject.gameObject.Tags.Add(tag);
    }

    public void RemoveTag()
    {
        if (Engine._selectedGameObject.gameObject.Tags.Count > 0)
        {
            Engine._selectedGameObject.gameObject.Tags.RemoveAt(selectedTag);
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

public class ComponentInstruction
{
    public Component? _component;
    public InstructionType? _instructionType;
}

public enum InstructionType
{
    Remove, Add
}