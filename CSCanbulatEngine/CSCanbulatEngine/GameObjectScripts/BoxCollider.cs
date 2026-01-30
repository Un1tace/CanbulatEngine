using System.Globalization;
using System.Numerics;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

public class BoxCollider : Component
{
    public Vector2 Size = new Vector2(1, 1);
    public Vector2 Offset = new Vector2(0, 0);
    public Vector2 CentreOfMassOffset = new Vector2(0, 0);
    public bool isStatic = false;

    /// <summary>
    /// If set as a trigger, only triggers the functions and doesnt act like a barrier.
    /// </summary>
    public bool isTrigger = false;

    public BoxCollider() : base("BoxCollider")
    {
        RequiredComponents.Add(typeof(Rigidbody));
    }

    /// <summary>
    /// Gets the AABB boundary of the collider
    /// </summary>
    /// <returns>AABB Vectors</returns>
    public (Vector2 Min, Vector2 Max) GetAabb()
    {
        var t = AttachedGameObject.GetComponent<Transform>();
        Vector2 worldScale = t.WorldScale;
        float rotation = t.WorldRotation;

        Vector2 half = (Size * worldScale) * 0.5f;

        Vector2[] corners =
        {
            new Vector2(-half.X, -half.Y),
            new Vector2(half.X, -half.Y),
            new Vector2(half.X, half.Y),
            new Vector2(-half.X, half.Y)
        };
        
        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);
        
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (var corner in corners)
        {
            Vector2 rotated = new Vector2(corner.X * cos - corner.Y * sin, corner.X * sin + corner.Y * cos) + (t.WorldPosition + Offset);
            
            min = Vector2.Min(min, rotated);
            max = Vector2.Max(max, rotated);
        }
        
        return (min, max);
    }

    /// <summary>
    /// Get the position of the centre of mass of the collider
    /// </summary>
    /// <returns>Position of centre of mass</returns>
    public Vector2 GetWorldCentreOfMass()
    {
        var t = AttachedGameObject.GetComponent<Transform>();
        float cos = MathF.Cos(t.WorldRotation);
        float sin = MathF.Sin(t.WorldRotation);

        Vector2 localCom = Offset + CentreOfMassOffset;
        Vector2 rotated = new Vector2(
            localCom.X * cos - localCom.Y * sin,
            localCom.X * sin +  localCom.Y * cos
            );
        
        return t.WorldPosition + rotated;
    }

    public override void RenderInspector()
    {
        ImGui.DragFloat2("Size", ref Size);
        
        ImGui.DragFloat2("Offset", ref Offset);
        
        ImGui.DragFloat2("Centre of Mass", ref CentreOfMassOffset);
        
        ImGui.Checkbox("Is Static", ref isStatic);
        
        ImGui.Checkbox("Is Trigger", ref isTrigger);
    }
    
        public override Dictionary<string, string> GetCustomProperties()
    {
        return new Dictionary<string, string>()
        {
            { "SizeX", Size.X.ToString(CultureInfo.InvariantCulture) },
            { "SizeY",  Size.Y.ToString(CultureInfo.InvariantCulture) },
            { "OffsetX", Offset.X.ToString(CultureInfo.InvariantCulture) },
            { "OffsetY", Offset.Y.ToString(CultureInfo.InvariantCulture) },
            { "CentreOfMassX", CentreOfMassOffset.X.ToString(CultureInfo.InvariantCulture) },
            { "CentreOfMassY", CentreOfMassOffset.Y.ToString(CultureInfo.InvariantCulture) },
            { "IsStatic", isStatic.ToString() },
            { "IsTrigger", isTrigger.ToString() }
        };
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        float sx = properties.TryGetValue("SizeX", out var sxStr)? float.Parse(sxStr, CultureInfo.InvariantCulture) : 0;
        float sy = properties.TryGetValue("SizeY", out var syStr)? float.Parse(syStr, CultureInfo.InvariantCulture) : 0;
        Size = new Vector2(sx, sy);
        
        float ox = properties.TryGetValue("OffsetX", out var oxStr) ? float.Parse(oxStr, CultureInfo.InvariantCulture) : 0;
        float oy = properties.TryGetValue("OffsetY", out var oyStr) ? float.Parse(oyStr, CultureInfo.InvariantCulture) : 0;
        Offset = new Vector2(ox, oy);
        
        float comx = properties.TryGetValue("CentreOfMassX", out var comxStr) ? float.Parse(comxStr, CultureInfo.InvariantCulture) : 0;
        float comy = properties.TryGetValue("CentreOfMassY", out var comyStr) ? float.Parse(comyStr, CultureInfo.InvariantCulture) : 0;
        CentreOfMassOffset = new Vector2(comx, comy);
        
        if (properties.TryGetValue("IsStatic", out var isStaticStr))
            isStatic = bool.Parse(isStaticStr);
        
        if (properties.TryGetValue("IsTrigger", out var isTriggerStr))
            isTrigger = bool.Parse(isTriggerStr);
    }
}