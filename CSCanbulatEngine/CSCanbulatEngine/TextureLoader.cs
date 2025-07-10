using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Silk.NET.Maths;

namespace CSCanbulatEngine;

//Loads images into the GPU ready for use in the game engine by creating opengl textures
public static class TextureLoader
{
    //Loads an image from the path and creates an OpenGl Texture
    public static unsafe uint Load(GL gl, string path, out Vector2D<int> size)
    {
        using var img = Image.Load<Rgba32>(path);
        // img.Mutate(x => x.Flip(FlipMode.Vertical));
        
        size = new Vector2D<int>(img.Width, img.Height);

        uint handle = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D ,handle);

        var pixels = new byte[4 * img.Width * img.Height];
        img.CopyPixelDataTo(pixels);

        fixed (void* data = pixels)
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)img.Width, (uint)img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }

        // Set texture parameters for wrapping and filtering.
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        
        // *** DEBUGGING STEP: Use simple linear filtering without mipmaps. ***
        // This is a common way to fix "texture unloadable" errors on macOS.
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        
        gl.BindTexture(TextureTarget.Texture2D, 0);

        return handle;
    }
}