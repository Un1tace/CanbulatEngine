using System.Globalization;
using System.Numerics;
using CSCanbulatEngine.Physics;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

/// <summary>
/// Simple 2D physics component that is driven by the ChernikovEngine.
/// </summary>
public class Rigidbody : Component
{
    public float Mass = 1f;
    public Vector2 Velocity = Vector2.Zero;
    public bool UseGravity = true;
    public bool IsKinematic = false;
    
    //Axis Constraints
    public bool FreezeX = false;
    public bool FreezeY = false;

    public float LinearDrag = 0.1f;

    public bool IsSimulated => !IsKinematic && Mass > 0f;

    public Rigidbody() : base("Rigidbody")
    {
        ChernikovEngine.Register(this);
    }

    public override void DestroyComponent()
    {
        ChernikovEngine.Unregister(this);
    }

    /// <summary>
    /// Steps through physics simulation. Called by ChernikovEngine
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Integrate(float deltaTime)
    {
        var transform = AttachedGameObject?.GetComponent<Transform>();
        if (transform == null) return;
        
        Vector2 acceleration = Vector2.Zero;
        
        if (UseGravity) acceleration += ChernikovEngine.Gravity * Mass;

        if (LinearDrag > 0f) acceleration += -Velocity * LinearDrag;

        Velocity += acceleration * deltaTime;

        Vector2 pos = transform.WorldPosition;

        if (!FreezeX) pos.X += Velocity.X * deltaTime;
        if (!FreezeY) pos.Y += Velocity.Y * deltaTime;
        
        transform.WorldPosition = pos;
    }
    
#if EDITOR
    // ------- Editor UI -------
    public override void RenderInspector()
    {
        ImGui.Text("Rigidbody (Chernikov Engine)");
        
        ImGui.DragFloat("Mass", ref Mass, 0.01f, 0.01f, 1000f);
        ImGui.Checkbox("Use Gravity", ref UseGravity);
        ImGui.Checkbox("Is Kinematic", ref IsKinematic);

        ImGui.Separator();
        ImGui.Text("Constraints");
        ImGui.Checkbox("Freeze X", ref FreezeX);
        ImGui.Checkbox("Freeze Y", ref FreezeY);
        
        ImGui.Separator();
        ImGui.Text("Velocity");
        ImGui.DragFloat2("Velocity", ref Velocity, 0.05f);
        
        ImGui.DragFloat("Linear Drag", ref LinearDrag, 0.01f, 0f, 10f);
    }
#endif
    
    // Serialisation
    public override Dictionary<string, string> GetCustomProperties()
    {
        return new Dictionary<string, string>()
        {
            { "Mass", Mass.ToString(CultureInfo.InvariantCulture) },
            { "Velocity.X", Velocity.X.ToString(CultureInfo.InvariantCulture) },
            { "Velocity.Y", Velocity.Y.ToString(CultureInfo.InvariantCulture) },
            { "UseGravity", UseGravity.ToString() },
            { "IsKinematic", IsKinematic.ToString() },
            { "FreezeX", FreezeX.ToString() },
            { "FreezeY", FreezeY.ToString() },
            { "LinearDrag", LinearDrag.ToString(CultureInfo.InvariantCulture) }
        };
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        if (properties.TryGetValue("Mass", out var massStr))
            Mass = float.Parse(massStr, CultureInfo.InvariantCulture);

        float vx = properties.TryGetValue("Velocity.X", out var vxStr)
            ? float.Parse(vxStr, CultureInfo.InvariantCulture)
            : 0f;
        float vy = properties.TryGetValue("Velocity.Y", out var vyStr)
            ? float.Parse(vyStr, CultureInfo.InvariantCulture)
            : 0f;
        Velocity = new Vector2(vx, vy);

        if (properties.TryGetValue("UseGravity", out var gStr))
            UseGravity = bool.Parse(gStr);
        if (properties.TryGetValue("IsKinematic", out var kStr))
            IsKinematic = bool.Parse(kStr);
        if (properties.TryGetValue("FreezeX", out var fxStr))
            FreezeX = bool.Parse(fxStr);
        if (properties.TryGetValue("FreezeY", out var fyStr))
            FreezeY = bool.Parse(fyStr);
        if (properties.TryGetValue("LinearDrag", out var dragStr))
            LinearDrag = float.Parse(dragStr, CultureInfo.InvariantCulture);
    }
}