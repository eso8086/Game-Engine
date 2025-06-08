using Silk.NET.OpenGL;
using System.Numerics;
using GLCtx = Silk.NET.OpenGL.GL;

namespace Engine;

public class Shader : IDisposable
{
    private uint _handle;
    private GLCtx GL;
    
    public Shader(GLCtx gl, string vertexPath, string fragmentPath)
    {
        GL = gl;

        uint vertexShaderHandle = LoadShader(ShaderType.VertexShader, vertexPath);
        uint fragmentShaderHandle = LoadShader(ShaderType.FragmentShader, fragmentPath);

        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vertexShaderHandle);
        GL.AttachShader(_handle, fragmentShaderHandle);
        GL.LinkProgram(_handle);

        GL.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status == 0)
        {
            Console.WriteLine($"Program failed to link: ${GL.GetProgramInfoLog(_handle)}");
        }

        GL.DetachShader(_handle, vertexShaderHandle);
        GL.DetachShader(_handle, fragmentShaderHandle);
        GL.DeleteShader(vertexShaderHandle);
        GL.DeleteShader(fragmentShaderHandle);
    }

    public void SetUniform(string name, int value)
    {
        int location = GL.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader");
        }
        GL.Uniform1(location, value);
    }
    
    
    public void SetUniform(string name, float value)
    {
        int location = GL.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader");
        }
        GL.Uniform1(location, value);
    }
    
    public unsafe void SetUniform(string name, Matrix4x4 matrix)
    {
        int location = GL.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader");
        }
        GL.UniformMatrix4(location, 1, false, (float*) &matrix);
    }

    public void Use()
    {
        GL.UseProgram(_handle);
    }
    private uint LoadShader(ShaderType type, string path)
    {
        string src = File.ReadAllText(path);
        uint handle = GL.CreateShader(type);
        GL.ShaderSource(handle, src);
        GL.CompileShader(handle);

        string infoLog = GL.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader: {infoLog}");
        }

        return handle;
    }

    public void Dispose()
    {
     GL.DeleteProgram(_handle);   
    }
}