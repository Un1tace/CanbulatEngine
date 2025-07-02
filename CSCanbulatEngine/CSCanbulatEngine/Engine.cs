namespace CSCanbulatEngine;

using Silk.NET;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
using System.Numerics;

public class Engine
{
    private static IWindow window;
    private static GL gl;
    
    // Variables for rendering
    private static uint Vbo;
    private static uint Vao;

    //Reference to Shader.cs class
    private static Shader shader;

    private static readonly float[] Vertices =
    {
        -0.5f, -0.5f, 0.0f,
        0.5f, -0.5f, 0.0f,
        0.0f, 0.5f, 0.0f
    };

    public void Run()
    {
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(1280, 720);
        options.Title = "Canbulat Engine";
        window = Window.Create(options);

        window.Load += OnLoad;
        window.Render += OnRender;
        window.Update += OnUpdate;
        window.Closing += OnClose;

        window.Run();
    }

    private unsafe void OnLoad()
    {
        gl = GL.GetApi(window);

        // For inputs
        IInputContext input = window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }
        
        // --- Setting up resources to render ---
        
        //Create and bind VAO and VBO
        Vao = gl.GenVertexArray();
        gl.BindVertexArray(Vao);

        Vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, Vbo);
        
        //Upload Vertices array to the VBO

        fixed (float* buf = Vertices)
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(Vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
        }
        
        //Create instance of shader class and pass file paths
        shader = new Shader(gl, "/Users/kaiafful/Developer/CSProjects/CanbulatEngine/CanbulatEngine/CSCanbulatEngine/CSCanbulatEngine/Shaders/shader.vert", "/Users/kaiafful/Developer/CSProjects/CanbulatEngine/CanbulatEngine/CSCanbulatEngine/CSCanbulatEngine/Shaders/shader.frag");
        
        // Tell OpenGL (Graphics API) how to read the data in the VBO
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
        gl.EnableVertexAttribArray(0);
    }

    private void OnUpdate(double deltaTime)
    {
        // Late Logic :)
    }

    private void OnRender(double deltaTime)
    {
        gl.ClearColor(Color.CornflowerBlue);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        
        // Bind VAO
        gl.BindVertexArray(Vao);
        
        //Use shader program
        shader.Use();
        
        gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
    }

    private void OnClose()
    {
        gl.DeleteBuffer(Vbo);
        gl.DeleteVertexArray(Vao);

        shader.Dispose();

        gl.Dispose();
    }

    
    // Gets the key hit on the keyboard and exits if its escape
    private void KeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        if (key == Key.Escape)
        {
            window.Close();
        }
    }
}