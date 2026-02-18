using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using CSCanbulatEngine.InfoHolders;
using CSCanbulatEngine.Mesh;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using Microsoft.IdentityModel.Tokens;

namespace CSCanbulatEngine.GameObjectScripts;

/// <summary>
/// Represents object in game world, with transform and mesh
/// </summary>
public class GameObject
{
    public string Name { get; set; }
    public List<Component> Components { get; }
    public List<string> Tags { get; set; }
    public int ID { get; set; }
    public GameObject? ParentObject { get; set; }
    public List<GameObject> ChildObjects { get; set; }
    
    public ObjectType objectType { get; set; }

    public GameObject(Mesh mesh, ObjectType objectType, string name = "GameObject", bool addCoreComponents = true)
    {
        Name = name;
        Components = new List<Component>();
        Tags = new List<string>();
        Tags.Add("GameObject");
        this.objectType = objectType;
        
        ChildObjects = new List<GameObject>();
        
        if (addCoreComponents)
        {
            //Add Core Components
            AddComponent(new Transform());
            AddComponent(new MeshRenderer(mesh));
        }
        
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
    
    public GameObject(string name, int id, Mesh meshToUse, ObjectType objectType)
    {
        Name = name;
        ID = id;
        Components = new List<Component>();
        Tags = new List<string>();
        ChildObjects = new List<GameObject>();
        this.objectType = objectType;
    
        // Add this new object to the scene
        if (!Engine.currentScene.GameObjects.Contains(this))
        {
            Engine.currentScene.GameObjects.Add(this);
        }
    }

    // Children/Parent handling
    public void MakeParentOfObject(GameObject childObject)
    {
        if (this != childObject && childObject != ParentObject)
        {
            Vector2 childObjPosition = childObject.GetComponent<Transform>().WorldPosition;
            if (childObject.ParentObject != null) childObject.RemoveParentObject();
            ChildObjects.Add(childObject);
            childObject.ParentObject = this;
            childObject.GetComponent<Transform>().WorldPosition = childObjPosition;
        }
    }

    public void MakeChildOfObject(GameObject parentObject)
    {
        if (!ChildObjects.Contains(parentObject) && this != parentObject)
        {
            Vector2 childObjPosition = this.GetComponent<Transform>().WorldPosition;
            if (ParentObject != null) RemoveParentObject();
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

    public Component? GetComponent(Type type)
    {
        return this.Components.Find(component => component.GetType() == type);
    }

    public bool HasComponent<T>() where T : Component
    {
        return Components.Any(component => component is T);
    }

    public bool HasComponent(Type type)
    {
        return this.Components.Any(component => component.GetType() == type);
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
        else
        {
            EngineLog.Log($"Component ({component.name}) cannot be removed");
        }
    }
    

    public void DeleteObject()
    {
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

        if (HasComponent<Camera>())
        {
            var camera = GetComponent<Camera>();

            if (Camera.Main == camera)
            {
                Camera.NullMainCamera();
            }
        }
        
        while (Components.Count > 0)
        {
            Components[0].DestroyComponent();
            Components.RemoveAt(0);
        }
        
        Engine.currentScene.GameObjects.Remove(this);
#if EDITOR
        if (Engine._selectedGameObject != null && Engine._selectedGameObject.gameObject == this)
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
    
    public static void RenderCreateObjectMenu(string superKey)
    {
        if (ImGui.BeginMenu("Object"))
        {
            if (ImGui.MenuItem("Create Square", superKey + "+A"))
            {
                new GameObject(ChunFactory.CreateQuad(), ObjectType.Quad);
            }

            if (ImGui.MenuItem("Create Circle"))
            {
                new GameObject(ChunFactory.CreateCircle(32), ObjectType.Circle);
            }

            if (ImGui.MenuItem("Create Triangle"))
            {
                new GameObject(ChunFactory.CreateTriangle(), ObjectType.Triangle);
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
        if (Engine._selectedGameObject != null && Engine._selectedGameObject.gameObject != null)
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
            
            // <Component Required, Components Which Require It>
            List<Type> requiredComponents = new();
            List<List<Type>> componentsWhichRequireIt = new();

            foreach (Component component in Engine._selectedGameObject.gameObject.Components)
            {
                if (!component.RequiredComponents.IsNullOrEmpty())
                {
                    foreach (Type type in component.RequiredComponents)
                    {
                        if (requiredComponents.Contains(type))
                        {
                            int index = requiredComponents.IndexOf(type);
                            componentsWhichRequireIt[index].Add(type);
                        }
                        else
                        {
                            requiredComponents.Add(type);
                            componentsWhichRequireIt.Add(new());
                            componentsWhichRequireIt.Last().Add(type);
                        }
                    }
                }
            }
            
            for (int i = 0; i < Engine._selectedGameObject.gameObject.Components.Count(); i++)
            {
                var component = Engine._selectedGameObject.gameObject.Components[i];
                ImGui.PushID(Engine._selectedGameObject.gameObject.Components.FindIndex(theComponent => theComponent == component));
                if (ImGui.CollapsingHeader(component.name, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    if (component.canBeDisabled)
                    {
                        ImGui.PushID(Engine._selectedGameObject.gameObject.Components.FindIndex(theComponent => theComponent == component));
                        ImGui.BeginDisabled(requiredComponents.Any(c => c == component.GetType()));
                        if (requiredComponents.Any(c => c == component.GetType())) component.isEnabled = true;
                        if (ImGui.Checkbox($"Is Enabled", ref component._isEnabled))
                        {
                            
                        }
                        ImGui.EndDisabled();
                        if (requiredComponents.Any(c => c == component.GetType()))
                        {
                            int requiredComponentIndex = requiredComponents.IndexOf(component.GetType());
                            List<Type> componentsRequiringIt = componentsWhichRequireIt[requiredComponentIndex];
                            string componentString = componentsRequiringIt[0].ToString().Split('.').Last();
                            foreach (var componentRelyingUpon in componentsRequiringIt)
                            {
                                if (componentRelyingUpon == componentsRequiringIt[0]) continue;
                                componentString += $", {componentRelyingUpon.ToString().Split('.').Last()}";
                            }
                            
                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text($"Cannot be disabled as components rely on it: {componentString}");
                                ImGui.EndTooltip();
                            }
                        }
                        ImGui.PopID();
                    }
                    if (component.canBeRemoved)
                    {
                        ImGui.PushID(Engine._selectedGameObject.gameObject.Components.FindIndex(theComponent => theComponent == component));
                        ImGui.BeginDisabled(requiredComponents.Any(c => c == component.GetType()));
                        if (ImGui.Button("Remove Component", new (ImGui.GetContentRegionAvail().X, ImGui.CalcTextSize("Add Component").Y + 5f)))
                        {
                            // Engine._selectedGameObject.gameObject.RemoveComponent(component);
                            nextInstruction._component = component;
                            nextInstruction._instructionType = InstructionType.Remove;

                            ImGui.PopID();
                            ImGui.PopID();
                            ImGui.EndDisabled();
                            continue;
                        }
                        ImGui.EndDisabled();
                        if (requiredComponents.Any(c => c == component.GetType()))
                        {
                            int requiredComponentIndex = requiredComponents.IndexOf(component.GetType());
                            List<Type> componentsRequiringIt = componentsWhichRequireIt[requiredComponentIndex];
                            string componentString = componentsRequiringIt[0].ToString().Split('.').Last();
                            foreach (var componentRelyingUpon in componentsRequiringIt)
                            {
                                if (componentRelyingUpon == componentsRequiringIt[0]) continue;
                                componentString += $", {componentRelyingUpon}";
                            }
                            
                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text($"Cannot be removed as components rely on it: {componentString.Split('.').Last()}");
                                ImGui.EndTooltip();
                            }
                        }
                        ImGui.PopID();
                    }
                    component.RenderInspector();
                }

                ImGui.PopID();
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

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("Holds information about object rendering. Like the mesh, colour and texture.");
                        ImGui.EndTooltip();
                    }
                }

                if (ImGui.MenuItem("CircuitScript"))
                {
                    var circuitScript = new CircuitScript();
                    Engine._selectedGameObject.gameObject.AddComponent(circuitScript);
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("Holds information for logic and what the object should execute at the start or during the game.");
                    ImGui.EndTooltip();
                }

                if (!Engine._selectedGameObject.gameObject.HasComponent<Rigidbody>())
                {
                    if (ImGui.MenuItem("Rigidbody"))
                    {
                        var rigidbody = new Rigidbody();
                        Engine._selectedGameObject.gameObject.AddComponent(rigidbody);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("Adds physics to the object and holds information about velocity, acceleration and gravity.");
                        ImGui.EndTooltip();
                    }
                }

                if (!Engine._selectedGameObject.gameObject.HasComponent<BoxCollider>())
                {
                    if (ImGui.MenuItem("BoxCollider"))
                    {
                        if (!Engine._selectedGameObject.gameObject.HasComponent<Rigidbody>())
                        {
                            var Rb = new Rigidbody();
                            Engine._selectedGameObject.gameObject.AddComponent(Rb);
                        }

                        var boxCollider = new BoxCollider();
                        Engine._selectedGameObject.gameObject.AddComponent(boxCollider);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("Allows the object to have collisions. (Rigidbody is required)");
                        ImGui.EndTooltip();
                    }
                }
                
                if (!Engine._selectedGameObject.gameObject.HasComponent<Camera>()) 
                { 
                    if (ImGui.MenuItem("Camera")) 
                    {
                        var camera = new Camera();
                        Engine._selectedGameObject.gameObject.AddComponent(camera); 
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("Used to show where to display gameobjects in the viewport");
                        ImGui.EndTooltip();
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

public enum ObjectType
{
    Quad, Triangle, Circle
}