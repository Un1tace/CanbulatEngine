namespace CSCanbulatEngine.GameObjectScripts;

public class Component
{
    
    private bool _isEnabled = true;
    public bool canBeDisabled = true;
    public bool canBeRemoved = true;
    
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
}