using System.Numerics;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

public class Component
{
    
    public bool _isEnabled = true;
    public bool canBeDisabled = true;
    public bool canBeRemoved = true;

    public GameObject AttachedGameObject;
    
    public bool isEnabled
    {
        get => _isEnabled;
        set => _isEnabled = !canBeDisabled || value;
    }
    
    public string name = "";

    public Component(string ComponentName)
    {
        name = ComponentName;
    }

    public virtual void RenderInspector()
    {
       ImGui.Text("Empty Component :)");
    }

    public virtual void DestroyComponent()
    {
    }
    
}