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
                if (Scale.X > Scale.Y)
                {
                    Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>().ImageResolution;
                    Scale.Y = Scale.X * ((float)resolution.Y / (float)resolution.X);
                }
                else
                {
                    Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>().ImageResolution;
                    Scale.X = Scale.Y * ((float)resolution.X / (float)resolution.Y);
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
            return AttachedGameObject.ParentObject is not null
                ? AttachedGameObject.ParentObject.GetComponent<Transform>().WorldPosition + _Position
                : _Position;} 
        set => _Position = (AttachedGameObject.ParentObject is not null? value - AttachedGameObject.ParentObject.GetComponent<Transform>().WorldPosition : value);
    }

    private Vector2 _Position = Vector2.Zero;
    
    public float RotationInDegrees
    {
        get { return Rotation * (180 / MathF.PI); }
        set { Rotation = value * (MathF.PI / 180f); }
    }

    //Rotation of object in radians
    public float Rotation = 0f;
    
    //Scale of an object in a 2D space
    public Vector2 Scale = Vector2.One;

    private bool ShowGlobalPosition = false;

    //Calculates and return the model matrix for this transform.
    //Model Matrix transforms the object from its local space to world space
    public Matrix4x4 GetModelMatrix()
    {
        Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(Scale.X, Scale.Y, 1f);
        Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationZ(Rotation);
        Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(WorldPosition.X, WorldPosition.Y, 0.0f);
        
        return scaleMatrix * rotationMatrix * translationMatrix;
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
                
        float rotation = Engine._selectedGameObject.gameObject.GetComponent<Transform>().RotationInDegrees;
        if (ImGui.DragFloat("Rotation", ref rotation, 0.01f))
        {
            Engine._selectedGameObject.gameObject.GetComponent<Transform>().RotationInDegrees = rotation;
        }
                
        Vector2 scale = Engine._selectedGameObject.gameObject.GetComponent<Transform>().Scale;
        if (ImGui.DragFloat2("Scale", ref scale, 0.05f))
        {
            // Engine._selectedGameObject.Transform.Scale = scale;
            if (Engine._selectedGameObject.gameObject.GetComponent<Transform>().ratioLocked)
            {
                if (scale.X != Engine._selectedGameObject.gameObject.GetComponent<Transform>().Scale.X)
                {
                    Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>().ImageResolution;
                    scale.Y = scale.X * ((float)resolution.Y / (float)resolution.X); //y/x
                }
                else if (scale.Y != Engine._selectedGameObject.gameObject.GetComponent<Transform>().Scale.Y)
                {
                    Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>().ImageResolution;
                    scale.X = scale.Y * ((float)resolution.X / (float)resolution.Y); //y/x
                }
            }
            
            Engine._selectedGameObject.gameObject.GetComponent<Transform>().Scale = scale;
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