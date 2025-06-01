using Silk.NET.OpenGL;
using GLCtx = Silk.NET.OpenGL.GL;

namespace Engine;

public class VertexArrayObject<TVertexType, TIndexType>: IDisposable
where TVertexType: unmanaged
where TIndexType: unmanaged
{
    private uint _handle;
    private GLCtx GL;


    public VertexArrayObject(GLCtx gl, BufferObject<TVertexType> VBO, BufferObject<TIndexType> EBO)
    {
        GL = gl;
        _handle = GL.GenVertexArray();
        Bind();
        VBO.Bind();
        EBO.Bind();
    }

    public void Bind()
    {
        GL.BindVertexArray(_handle);
    }

    public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offSet)
    {
        GL.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(TVertexType), (void*) (offSet * sizeof(TVertexType)));
        GL.EnableVertexAttribArray(index);
    }
    
    public void Dispose()
    {
        GL.DeleteVertexArray(_handle);
    }
}