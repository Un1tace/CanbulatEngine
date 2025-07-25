using Silk.NET.OpenGL;

namespace CSCanbulatEngine.GameObjectScripts;

//Holds information about the mesh, vertex data and openGl buffer objects
public class Mesh
{
    private readonly GL _gl;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _ebo;
    private readonly uint _indexCount;

    public unsafe Mesh(GL gl, float[] vertices, uint[] indices)
    {
        _gl = gl;
        _indexCount = (uint)indices.Length;
        
        //Create and bind VAO (Vertex array object)
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);
        
        //Create and bind the VBO (Vertex buffer object) then upload the vertex data to GPU
        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* buf = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
        }
        
        //Create and bind EBO (Element buffer object) and upload index data
        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* buf = indices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
        }

        const uint vertexSize = 5 * sizeof(float);
        
        //Set the vertex attrib pointer to tell OpenGL how to interpret the vertex data (location 0)
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, null);
        _gl.EnableVertexAttribArray(0);
        
        //Vertex attribute for texture coords (location 1)
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)(3 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);
        
        //Unbind the VAO to prevent accidental changes
        _gl.BindVertexArray(0);
    }
    
    //Binds the mesh's VAO and draws it
    public unsafe void Draw()
    {
        _gl.BindVertexArray(_vao);
        _gl.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, null);
    }
    
    //Cleans up the OpenGL buffers
    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
    }
}