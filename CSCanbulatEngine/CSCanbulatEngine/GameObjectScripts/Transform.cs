using System.Numerics;
using ImGuiNET;

namespace CSCanbulatEngine.GameObjectScripts;

//Holds position, rotation and scale of an object
public class Transform : Component
{
    public Transform() : base("Transform")
    {
        base.canBeDisabled = false;
        base.canBeRemoved = false;
    }
    //Pos of object in 2D space
    public Vector2 Position = Vector2.Zero;
    
    public float RotationInDegrees
    {
        get { return Rotation * (180 / MathF.PI); }
        set { Rotation = value * (MathF.PI / 180f); }
    }

    //Rotation of object in radians
    public float Rotation = 0f;
    
    //Scale of an object in a 2D space
    public Vector2 Scale = Vector2.One;

    //Calculates and return the model matrix for this transform.
    //Model Matrix transforms the object from its local space to world space
    public Matrix4x4 GetModelMatrix()
    {
        Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(Scale.X, Scale.Y, 1f);
        Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationZ(Rotation);
        Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(Position.X, Position.Y, 0.0f);
        
        return scaleMatrix * rotationMatrix * translationMatrix;
    }

    #if EDITOR
    public override void RenderInspector()
    {
        Vector2 position = Engine._selectedGameObject.Transform.Position;
        if (ImGui.DragFloat2("Position", ref position, 0.05f))
        {
            Engine._selectedGameObject.Transform.Position = position;
        }
                
        float rotation = Engine._selectedGameObject.Transform.RotationInDegrees;
        if (ImGui.DragFloat("Rotation", ref rotation, 0.01f))
        {
            Engine._selectedGameObject.Transform.RotationInDegrees = rotation;
        }
                
        Vector2 scale = Engine._selectedGameObject.Transform.Scale;
        if (ImGui.DragFloat2("Scale", ref scale, 0.05f))
        {
            Engine._selectedGameObject.Transform.Scale = scale;
        }
    }
#endif
}