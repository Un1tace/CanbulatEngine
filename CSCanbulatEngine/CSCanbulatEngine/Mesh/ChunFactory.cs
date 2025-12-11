namespace CSCanbulatEngine.Mesh;

/// <summary>
/// Mesh Factory for the Canbulat Engine. Makes multiple shapes for meshes.
/// </summary>
public class ChunFactory
{
    /// <summary>
    /// Creates a triangle mesh
    /// </summary>
    /// <returns></returns>
    public static GameObjectScripts.Mesh CreateTriangle()
    {
        // float[] vertices = {
        //     // Position (X, Y, Z)    // UV (U, V)
        //     0.0f,  0.5f, 0.0f,      0.5f, 1.0f, // Top
        //     -Single.Sqrt(0.25f/2f), -Single.Sqrt(0.25f/2f), 0.0f,      0.0f, 0.0f, // Bottom Left
        //     Single.Sqrt(0.25f/2f), -Single.Sqrt(0.25f/2f), 0.0f,      1.0f, 0.0f  // Bottom Right
        // };
        //
        // uint[] indices = { 0, 1, 2 };

        return CreateCircle(3);
    }

    /// <summary>
    /// Creates a Quad Mesh
    /// </summary>
    /// <returns></returns>
    public static GameObjectScripts.Mesh CreateQuad()
    {
        float[] vertices =
        {
            // Position           // UV
            0.5f,  0.5f, 0.0f,   1.0f, 0.0f, // Top Right (0)
            0.5f, -0.5f, 0.0f,   1.0f, 1.0f, // Bottom Right (1)
            -0.5f, -0.5f, 0.0f,   0.0f, 1.0f, // Bottom Left (2)
            -0.5f,  0.5f, 0.0f,   0.0f, 0.0f  // Top Left (3)
        };

        uint[] indices = { 0, 1, 2, 2, 3, 0 };
        return new GameObjectScripts.Mesh(Engine.gl, vertices, indices);
    }
    
    /// <summary>
    /// Creates a circle with an amount of segments
    /// </summary>
    /// <param name="segments"></param>
    /// <returns></returns>
    public static GameObjectScripts.Mesh CreateCircle(int segments)
    {
        float radius = 1;
        List<float> vertices = new List<float>();
        List<uint> indices = new List<uint>();
        
        vertices.AddRange(new[] { 0f, 0f, 0f, 0.5f, 0.5f });

        float step = (float)(2 * Math.PI) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * step;
            float x = (float)Math.Sin(angle) * 0.5f;
            float y = (float)Math.Cos(angle) * 0.5f;

            float u = (x / (radius * 2)) + 0.5f;
            float v = (y / (radius * 2)) + 0.5f;
            
            vertices.AddRange(x, y, 0f, u, v);
        }

        for (uint i = 1; i <= segments; i++)
        {
            indices.Add(0);
            indices.Add((uint)i);
            indices.Add((uint)i + 1);
        }
        
        return new GameObjectScripts.Mesh(Engine.gl, vertices.ToArray(), indices.ToArray());
    }
}