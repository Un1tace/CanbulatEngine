namespace CSCanbulatEngine;

using Silk.NET.OpenGL;
using System;
using System.IO;

// Shader, needed code to put on gpu to show cool stuff on screen
public class Shader
{
    // Identifiers for shader program
    private uint _handle;
    private GL _gl;

    public Shader(GL gl, string vertexPath, string fragmentPath)
    {
        _gl = gl;
        
        
        string vertexFullPath = Path.Combine(AppContext.BaseDirectory, vertexPath);
        string fragmentFullPath = Path.Combine(AppContext.BaseDirectory, fragmentPath);

        if (!File.Exists(vertexFullPath))
        {
            throw new FileNotFoundException($"Vertex shader not found at: {vertexFullPath}");
        }
        
        if (!File.Exists(fragmentFullPath))
        {
            throw new FileNotFoundException($"Fragment shader not found at: {fragmentFullPath}");
        }

        // Load code from files
        string vertexShaderSource = File.ReadAllText(vertexFullPath);
        string fragmentShaderSource = File.ReadAllText(fragmentFullPath);

        // Create and compile the shaders
        uint vertexShader = LoadShader(ShaderType.VertexShader, vertexShaderSource);
        uint fragmentShader = LoadShader(ShaderType.FragmentShader, fragmentShaderSource);
        
        // Create the shader program and link them together
        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertexShader);
        _gl.AttachShader(_handle, fragmentShader);
        _gl.LinkProgram(_handle);
        
        // Checking for more linking errors
        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program link failed: {_gl.GetProgramInfoLog(_handle)}");
        }
        
        //Delete shaders after linked
        _gl.DetachShader(_handle, vertexShader);
        _gl.DetachShader(_handle, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    private uint LoadShader(ShaderType type, string source)
    {
        uint handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, source);
        _gl.CompileShader(handle);
        
        // Just checking for errors :)
        string infoLog = _gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}: {infoLog}");
        }

        return handle;
    }

    //Tell OpenGL to use this program to render stuff 
    public void Use()
    {
        _gl.UseProgram(_handle);
    }
    
    // Clean up the shader program when done so it doesn't leak
    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }
}