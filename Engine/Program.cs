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
    static IKeyboard PrimaryKeyboard;
    
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
    static Vector3 CameraFront = new(0f, 0f, -1.0f);
    static Vector3 CameraUp = Vector3.UnitY;
    static Vector3 CameraDirection = Vector3.Zero; 
    static float CameraYaw = -90f;
    static float CameraPitch;
    static float CameraZoom = 45f;

    static Vector2 LastMousePosition;
    

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
            Size = new Vector2D<int>(1280, 720),
            Title = "Engine",
            // WindowBorder = WindowBorder.Fixed,
            IsVisible = false,
        };
        Window = SilkWindow.Create(options);
        
        Window.Load += OnLoad;
        Window.Update += OnUpdate;
        Window.Render += OnRender;
        Window.Closing += OnClose;
        Window.FramebufferResize += OnFrameBufferResize;

        Window.Run();
        Window.Dispose();
    }

    static void OnFrameBufferResize(Vector2D<int> newSize)
    {
        GL.Viewport(newSize);
    }
    static void OnMouseMove(IMouse mouse, Vector2 position)
    {
        var lookSensitivity = 0.1f;
        if (LastMousePosition == default)
                LastMousePosition = position;
        else
        {
            var xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
            var yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
            LastMousePosition = position;
            
            CameraYaw += xOffset;
            CameraPitch -= yOffset;

            CameraPitch = Math.Clamp(CameraPitch, -89f, 89f);

            CameraDirection.X = MathF.Cos(Scalar.DegreesToRadians(CameraYaw)) * MathF.Cos(Scalar.DegreesToRadians(CameraPitch));
            CameraDirection.Y = MathF.Sin(Scalar.DegreesToRadians(CameraPitch));
            CameraDirection.Z = MathF.Sin(Scalar.DegreesToRadians(CameraYaw)) * MathF.Cos(Scalar.DegreesToRadians(CameraPitch));
            CameraFront = Vector3.Normalize(CameraDirection);
        }
    }

    static void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
        CameraZoom = Math.Clamp(CameraZoom - scrollWheel.Y, 1.0f, 45f);
    }

    static void KeyDown(IKeyboard keyboard, Key key, int code)
    {
        if (key == Key.Escape)
        {
            Window.Close();
        }
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
        
        PrimaryKeyboard = input.Keyboards.FirstOrDefault();
        
        if (PrimaryKeyboard is not null)
                PrimaryKeyboard.KeyDown += KeyDown;

        foreach (IMouse mouse in input.Mice)
        {
            mouse.Cursor.CursorMode = CursorMode.Raw;
            mouse.MouseMove += OnMouseMove;
            mouse.Scroll += OnMouseWheel;
        }


        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    static void OnUpdate(double delta)
    {

        var moveSpeed = 2.5f * (float)delta;

        if (PrimaryKeyboard.IsKeyPressed(Key.W))
        {
            CameraPosition += moveSpeed * CameraFront;
        }

        if (PrimaryKeyboard.IsKeyPressed(Key.S))
        {
            CameraPosition -= moveSpeed * CameraFront;
        }

        if (PrimaryKeyboard.IsKeyPressed(Key.A))
        {
            CameraPosition -= Vector3.Normalize(Vector3.Cross(CameraFront, CameraUp)) * moveSpeed;
        }

        if (PrimaryKeyboard.IsKeyPressed(Key.D))
        {
            CameraPosition += Vector3.Normalize(Vector3.Cross(CameraFront, CameraUp)) * moveSpeed;
        }
        
        if(Transforms.Count > 0)
            Transforms[0].Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0f, 0f, 1.0f), Scalar.DegreesToRadians((float) Window.Time * 30));
    }
    
    static unsafe void OnRender(double delta)
    {
        GL.ClearColor(Color.Black);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        VAO.Bind();
        Texture.Bind();
        Shader.Use();

        Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(CameraPosition, CameraPosition + CameraFront, CameraUp);
        Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
            Scalar.DegreesToRadians(CameraZoom), (float) Window.Size.X / Window.Size.Y, 0.1f, 100.0f);
        
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