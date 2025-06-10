using System.Drawing;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using GLCtx = Silk.NET.OpenGL.GL;
using SilkWindow = Silk.NET.Windowing.Window;

namespace Engine;

class Program
{
    
    // silk stuff
    static IWindow Window;
    static GLCtx GL;
    
    // gl stuff
    static VertexArrayObject<float, uint> VAO;
    static BufferObject<uint> EBO;
    static BufferObject<float> VBO;
    static Shader Shader;
    static Texture Texture;
    
    // entity related stuff
    static List<Transform> Transforms = new();
    
    //camera stuff
    // TODO for me: learn cross product order thing
    static Vector3 CameraPosition = new(0.0f, 0.0f, 3.0f);
    static Vector3 CameraTarget = new(0.0f);
    static Vector3 CameraDirection = Vector3.Normalize(CameraPosition - CameraTarget); 
    static Vector3 CameraRight = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, CameraDirection));
    static Vector3 CameraUp = Vector3.Cross(CameraDirection, CameraRight);
    

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
            IsVisible = false,
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
        
        Transforms.Add( new Transform());
        
        VBO = new BufferObject<float>(GL, Vertices, BufferTargetARB.ArrayBuffer);
        EBO = new BufferObject<uint>(GL, Indices, BufferTargetARB.ElementArrayBuffer);
        VAO = new VertexArrayObject<float, uint>(GL, VBO, EBO);

        VAO.Bind();
        VAO.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
        VAO.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

        Texture = new Texture(GL, "mario.png");
        Shader = new Shader(GL, "shader.vert", "shader.frag");
        
        IInputContext input = Window.CreateInput();
        foreach (IKeyboard keyboard in input.Keyboards)
        {
            keyboard.KeyDown += (keyboard1, key, code) =>
            {
                if (key == Key.Escape)
                {
                    Window.Close();
                }
            };
        }


        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    static void OnUpdate(double delta)
    {
        if(Transforms.Count > 0)
            Transforms[0].Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0f, 0f, 1.0f), Scalar.DegreesToRadians((float) Window.Time * 180));
    }
    
    static unsafe void OnRender(double delta)
    {
        GL.ClearColor(Color.Black);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        VAO.Bind();
        Texture.Bind();
        Shader.Use();

        Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(CameraPosition, CameraTarget, CameraUp);
        Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
            Scalar.DegreesToRadians(45.0f), (float) Window.Size.X / Window.Size.Y, 0.1f, 100.0f);
        
        Shader.SetUniform("uView", viewMatrix);
        Shader.SetUniform("uProjection", projectionMatrix);
        
        foreach (var transform in Transforms)
        {
            Shader.SetUniform("uTrans", transform.Matrix);
            GL.DrawElements(PrimitiveType.Triangles, (uint) Indices.Length, DrawElementsType.UnsignedInt, (void*) 0);
        }
        
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