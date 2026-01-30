using System.Globalization;
using System.Numerics;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using Silk.NET.Maths;

namespace CSCanbulatEngine.GameObjectScripts;

//Holds position, rotation and scale of an object
public class Transform : Component
{
#if EDITOR
    public bool ratioLocked
    {
        get { return _ratioLocked; }
        set
        {
            _ratioLocked = value;
            
            //Quality of life feature
            if (ratioLocked)
            {
                if (WorldScale.X > WorldScale.Y)
                {
                    Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>().ImageResolution;
                    WorldScale = new Vector2(WorldScale.X, WorldScale.X * ((float)resolution.Y / (float)resolution.X));
                }
                else
                {
                    Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>().ImageResolution;
                    WorldScale = new Vector2(WorldScale.Y * ((float)resolution.X / (float)resolution.Y), WorldScale.Y);
                }
            }
        }
    }
#endif

    private bool _ratioLocked = false;
    public Transform() : base("Transform")
    {
        base.canBeDisabled = false;
        base.canBeRemoved = false;
    }
    //Pos of object in 2D space
    // public Vector2 Position = Vector2.Zero;
    public Vector2 LocalPosition
    {
        get { return _Position;}
        set { _Position = value; }
    }
    public Vector2 WorldPosition
    {
        get
        {
            if (AttachedGameObject.ParentObject != null)
            {
                Transform? parentTransform = AttachedGameObject.ParentObject.GetComponent<Transform>();
                float cos = MathF.Cos(parentTransform.WorldRotation);
                float sin = MathF.Sin(parentTransform.WorldRotation);

                Vector2 rotatedVector =
                    new Vector2(_Position.X * cos - _Position.Y * sin, _Position.X * sin + _Position.Y * cos);

                return AttachedGameObject.ParentObject.GetComponent<Transform>().WorldPosition + rotatedVector;
            }
            else return _Position;
        } 
        set
        {
            if (AttachedGameObject.ParentObject is not null)
            {
                Transform parentTransform = AttachedGameObject.ParentObject.GetComponent<Transform>();
                Vector2 worldOffset = value - parentTransform.WorldPosition;
                float cos = MathF.Cos(-parentTransform.WorldRotation);
                float sin = MathF.Sin(-parentTransform.WorldRotation);
                _Position = new Vector2(
                    worldOffset.X * cos - worldOffset.Y * sin,
                    worldOffset.X * sin + worldOffset.Y * cos
                );
            }
            else
            {
                _Position = value;
            }
        }
    }

    private Vector2 _Position = Vector2.Zero;
    
    public float WorldRotationInDegrees
    {
        get { return WorldRotation * (180 / MathF.PI); }
        set { WorldRotation = value * (MathF.PI / 180f); }
    }

    public float LocalRotationInDegrees
    {
        get { return LocalRotation * (180 / MathF.PI); }
        set { LocalRotation = value * (MathF.PI / 180f); }
    }

    //Rotation of object in radians
    public float WorldRotation
    {
        get
        {
            if (AttachedGameObject.ParentObject is not null)
            {
                Transform? parentTransform = AttachedGameObject.ParentObject.GetComponent<Transform>();
                return _Rotation + parentTransform.WorldRotation;
            }
            else return _Rotation;
        }
        set 
        {
            if (AttachedGameObject.ParentObject is not null)
            {
                Transform? parentTransform = AttachedGameObject.ParentObject.GetComponent<Transform>();
                _Rotation = value - parentTransform.WorldRotation;
            }
            else _Rotation = value;
        }
    }

    public float LocalRotation
    {
        get => _Rotation; 
        set => _Rotation = value;
    }

    private float _Rotation = 0f;
    
    //Scale of an object in a 2D space
    public Vector2 WorldScale
    {
        get
        {
            if (AttachedGameObject.ParentObject is not null)
            {
                Transform? parentTransform = AttachedGameObject.ParentObject.GetComponent<Transform>();
                return _Scale * parentTransform.WorldScale;
            }

            return _Scale;
        }

        set
        {
            if (AttachedGameObject.ParentObject is not null)
            {
                Transform? parentTransform = AttachedGameObject.ParentObject.GetComponent<Transform>();
                Vector2 parentWorld = parentTransform.WorldScale;
                _Scale = new Vector2(
                    parentWorld.X != 0 ? value.X / parentWorld.X : value.X,
                    parentWorld.Y != 0 ? value.Y / parentWorld.Y : value.Y
                    );
            }
            else _Scale = value;
        }
    }

    public Vector2 LocalScale
    {
        get { return _Scale;}
        set { _Scale = value; }
    }

    public Vector2 _Scale = Vector2.One;

    private bool ShowGlobalPosition = false;

    private bool ShowGlobalRotation = false;
    
    private bool ShowGlobalScale = false;

    //Calculates and return the model matrix for this transform.
    //Model Matrix transforms the object from its local space to world space
    public Matrix4x4 GetModelMatrix()
    {
        Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(WorldScale.X, WorldScale.Y, 1f);
        Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationZ(WorldRotation);
        Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(WorldPosition.X, WorldPosition.Y, 0.0f);
        
        return scaleMatrix * rotationMatrix * translationMatrix;
    }

    public override Dictionary<string, string> GetCustomProperties()
    {
        return new Dictionary<string, string>
        {
            { "Position.X", WorldPosition.X.ToString(CultureInfo.InvariantCulture) },
            { "Position.Y", WorldPosition.Y.ToString(CultureInfo.InvariantCulture) },
            { "Rotation", WorldRotation.ToString(CultureInfo.InvariantCulture) },
            { "Scale.X", WorldScale.X.ToString(CultureInfo.InvariantCulture) },
            { "Scale.Y", WorldScale.Y.ToString(CultureInfo.InvariantCulture) }
        };
    }

    public override void SetCustomProperties(Dictionary<string, string> properties)
    {
        WorldPosition = new Vector2(float.Parse(properties["Position.X"], CultureInfo.InvariantCulture), float.Parse(properties["Position.Y"], CultureInfo.InvariantCulture));
        WorldRotation = float.Parse(properties["Rotation"], CultureInfo.InvariantCulture);
        WorldScale = new Vector2(float.Parse(properties["Scale.X"], CultureInfo.InvariantCulture), float.Parse(properties["Scale.Y"], CultureInfo.InvariantCulture));
    }

    #if EDITOR
    public override void RenderInspector()
    {
        if (ShowGlobalPosition)
        {
            Vector2 position = Engine._selectedGameObject.gameObject.GetComponent<Transform>().WorldPosition;
            if (ImGui.DragFloat2("Position", ref position, 0.05f))
            {
                Engine._selectedGameObject.gameObject.GetComponent<Transform>().WorldPosition = position;
            }
        }
        else
        {
            Vector2 position = Engine._selectedGameObject.gameObject.GetComponent<Transform>().LocalPosition;
            if (ImGui.DragFloat2("Position", ref position, 0.05f))
            {
                Engine._selectedGameObject.gameObject.GetComponent<Transform>().LocalPosition = position;
            }
        }

        if (AttachedGameObject.ParentObject is not null)
        {
            if (ImGui.Checkbox("Show Global/World Space Position", ref ShowGlobalPosition))
            {
                
            }
        }

        if (ShowGlobalRotation)
        {
            float rotation = Engine._selectedGameObject.gameObject.GetComponent<Transform>().WorldRotationInDegrees;
            if (ImGui.DragFloat("Rotation", ref rotation, 0.01f))
            {
                Engine._selectedGameObject.gameObject.GetComponent<Transform>().WorldRotationInDegrees = rotation;
            }
        }
        else
        {
            float rotation = Engine._selectedGameObject.gameObject.GetComponent<Transform>().LocalRotationInDegrees;
            if (ImGui.DragFloat("Rotation", ref rotation, 0.01f))
            {
                Engine._selectedGameObject.gameObject.GetComponent<Transform>().LocalRotationInDegrees = rotation;
            }
        }

        if (AttachedGameObject.ParentObject is not null)
        {
            if (ImGui.Checkbox("Show Global/Local Space Rotation", ref ShowGlobalRotation))
            {
                
            }
        }

        if (ShowGlobalScale)
        {
            Vector2 worldScale = Engine._selectedGameObject.gameObject.GetComponent<Transform>().WorldScale;
            if (ImGui.DragFloat2("Scale", ref worldScale, 0.05f))
            {
                // Engine._selectedGameObject.Transform.Scale = scale;
                if (Engine._selectedGameObject.gameObject.GetComponent<Transform>().ratioLocked)
                {
                    if (worldScale.X != Engine._selectedGameObject.gameObject.GetComponent<Transform>().WorldScale.X)
                    {
                        Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>()
                            .ImageResolution;
                        worldScale.Y = worldScale.X * ((float)resolution.Y / (float)resolution.X); //y/x
                    }
                    else if (worldScale.Y !=
                             Engine._selectedGameObject.gameObject.GetComponent<Transform>().WorldScale.Y)
                    {
                        Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>()
                            .ImageResolution;
                        worldScale.X = worldScale.Y * ((float)resolution.X / (float)resolution.Y); //y/x
                    }
                }

                Engine._selectedGameObject.gameObject.GetComponent<Transform>().WorldScale = worldScale;
            }
        }
        else
        {
            Vector2 localScale = Engine._selectedGameObject.gameObject.GetComponent<Transform>().LocalScale;
            if (ImGui.DragFloat2("Scale", ref localScale, 0.05f))
            {
                // Engine._selectedGameObject.Transform.Scale = scale;
                if (Engine._selectedGameObject.gameObject.GetComponent<Transform>().ratioLocked)
                {
                    if (localScale.X != Engine._selectedGameObject.gameObject.GetComponent<Transform>().LocalScale.X)
                    {
                        Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>()
                            .ImageResolution;
                        localScale.Y = localScale.X * ((float)resolution.Y / (float)resolution.X); //y/x
                    }
                    else if (localScale.Y !=
                             Engine._selectedGameObject.gameObject.GetComponent<Transform>().LocalScale.Y)
                    {
                        Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>()
                            .ImageResolution;
                        localScale.X = localScale.Y * ((float)resolution.X / (float)resolution.Y); //y/x
                    }
                }

                Engine._selectedGameObject.gameObject.GetComponent<Transform>().LocalScale = localScale;
            }
        }

        if (AttachedGameObject.ParentObject is not null)
        {
            ImGui.Checkbox("Show Global/World Space Scale", ref ShowGlobalScale);
        }

        if (AttachedGameObject.GetComponent<MeshRenderer>() != null)
        {
            if (AttachedGameObject.GetComponent<MeshRenderer>().TextureID != 0 &&
                LoadIcons.icons.ContainsKey("Lock.png"))
            {
                if (ImGui.ImageButton("Lock Scale Ratio", (IntPtr)LoadIcons.icons["Lock.png"],
                        new Vector2(25, 25), Vector2.Zero, Vector2.One, Vector4.Zero,
                        ratioLocked ? Vector4.One : new Vector4(1, 1, 1, 0.5f)))
                {
                    ratioLocked = !ratioLocked;
                }

                ImGui.SameLine();
                ImGui.Text($"Image Ratio Locked: {ratioLocked}");
            }
            else
            {
                ratioLocked = false;
            }
        }
    }
#endif
}