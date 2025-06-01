using GLCtx = Silk.NET.OpenGL.GL;
using Silk.NET.OpenGL;
namespace Engine;

public class BufferObject<TDataType> : IDisposable
where TDataType: unmanaged
{

    private uint _handle;
    private BufferTargetARB _bufferType;
    private GLCtx GL;

    public unsafe BufferObject(GLCtx gl, Span<TDataType> data, BufferTargetARB bufferType)
    {
        GL = gl;
        _bufferType = bufferType;
        _handle = GL.GenBuffer();
        Bind();
        fixed (void* d = data)
        {
            GL.BufferData(bufferType, (nuint) (data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
    }

    public void Bind()
    {
        GL.BindBuffer(_bufferType, _handle);
    }
    
    public void Dispose()
    {
        GL.DeleteBuffer(_handle);
    }
}