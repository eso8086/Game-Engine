using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using GLCtx = Silk.NET.OpenGL.GL;
using SilkWindow = Silk.NET.Windowing.Window;

namespace Engine;

class Program
{
    static IWindow Window;
    static GLCtx GL;
    static VertexArrayObject<float, uint> VAO;
    static BufferObject<uint> EBO;
    static BufferObject<float> VBO;
    static Shader Shader;
    static Texture Texture;

    static readonly float[] Vertices =
    {
        // X    Y     Z     S      T
        -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom-left - bottom-left
         0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom-right - bottom-right
         0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top-right - top-right
         -0.5f, 0.5f, 0.0f, 0.0f, 1.0f //  top-left - top-left
    };

    static readonly uint[] Indices =
    {
        0u, 1u, 2u,
        2u, 3u, 0u
    };
     
    static void Main(string[] args)
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "Engine",
            WindowBorder = WindowBorder.Fixed,
            IsVisible = false
        };
        Window = SilkWindow.Create(options);
        
        Window.Load += OnLoad;
        Window.Update += OnUpdate;
        Window.Render += OnRender;
        Window.Closing += OnClose;

        Window.Run();
        Window.Dispose();
    }
    static void OnLoad()
    {
        
        GL = Window.CreateOpenGL();
        Window.Center();
        Window.IsVisible = true;
        
        
        VBO = new BufferObject<float>(GL, Vertices, BufferTargetARB.ArrayBuffer);
        EBO = new BufferObject<uint>(GL, Indices, BufferTargetARB.ElementArrayBuffer);
        VAO = new VertexArrayObject<float, uint>(GL, VBO, EBO);

        VAO.Bind();
        VAO.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
        VAO.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

        Texture = new Texture(GL, "mario.png");
        Shader = new Shader(GL, "shader.vert", "shader.frag");
        
        IInputContext input = Window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += (IKeyboard keyboard, Key key, int code) =>
            {
                if (key == Key.Escape) Window.Close();
            };
        }
        
        
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    static void OnUpdate(double delta)
    {
        
    }
    
    static unsafe void OnRender(double delta)
    {
        GL.ClearColor(Color.Black);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        VAO.Bind();
        Texture.Bind();
        Shader.Use();

        Matrix4X4<float> rotation = Matrix4X4.CreateRotationZ(Scalar.DegreesToRadians(0f));
        Matrix4X4<float> scale = Matrix4X4.CreateScale(1f);
        Matrix4X4<float> translation = Matrix4X4.CreateTranslation(new Vector3D<float>(0.0f, 0.0f, 0.0f));
        
        Matrix4X4<float> trans = scale * rotation * translation;
        
        Shader.SetUniform("uTrans", trans);
        
        GL.DrawElements(PrimitiveType.Triangles, (uint) Indices.Length, DrawElementsType.UnsignedInt, (void*) 0);
    }
    
    static void OnClose()
    {
        VBO.Dispose();
        EBO.Dispose();
        VAO.Dispose();
        Texture.Dispose();
        Shader.Dispose();
    }
}