using StbImageSharp;
using Silk.NET.OpenGL;
using GLCtx = Silk.NET.OpenGL.GL;

namespace Engine;

public class Texture: IDisposable
{
    private uint _handle;
    private GLCtx GL;


    public unsafe Texture(GLCtx gl, string texPath)
    {
        GL = gl;
        _handle = GL.GenTexture();
        Bind();
        
        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(texPath), ColorComponents.RedGreenBlueAlpha);
        fixed (byte* ptr = result.Data)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint) result.Width, (uint) result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }
        
        SetParameters();
    }

    public unsafe Texture(GLCtx gl, Span<byte> data, uint width, uint height)
    {

        GL = gl;
        _handle = GL.GenTexture();
        Bind();
        
        fixed(byte* ptr = data)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }
    }


    public void Bind(TextureUnit textureUnit = TextureUnit.Texture0)
    {
        GL.ActiveTexture(textureUnit);
        GL.BindTexture(TextureTarget.Texture2D, _handle);
    }
    
    public void Dispose()
    {
        GL.DeleteTexture(_handle);
    }

    private void SetParameters()
    {
        GL.TextureParameter(_handle, TextureParameterName.TextureBaseLevel, 0);
        GL.TextureParameter(_handle, TextureParameterName.TextureMaxLevel, 8);
        
        GL.TextureParameter(_handle, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
        GL.TextureParameter(_handle, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);

        GL.TextureParameter(_handle, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.NearestMipmapLinear);
        GL.TextureParameter(_handle, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
        
        GL.GenerateMipmap(TextureTarget.Texture2D);
    }
    
}