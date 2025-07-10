using System.Numerics;

namespace CSCanbulatEngine.GameObjectScripts;

//Holds position, rotation and scale of an object
public class Transform() : Component("Transform")
{
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
}