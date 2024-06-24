using ImGuiNET;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Numerics;

namespace Grafika_Projekt
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor = new();
        private static CubeArrangementModel cubeArrangementModel = new();
        private static SquirrelArrangementModel squirrelArrangementModel = new();
        private static List<AppleArrangementModel> appleArrangementModels = new List<AppleArrangementModel>();
        

        private static IWindow window;

        private static IInputContext inputContext;

        private static GL Gl;

        private static ImGuiController controller;

        private static uint program;

        private static GlObject teapot;

        private static GlObject table;

        private static GlObject squirrel;

        private static GlObject apple;

        private static GlCube glCubeRotating;

        private static GlCube skyBox;

        private static GlObject glSphere;

        private static GlObject glPlate;

        private static bool KeyW = false;
        private static bool KeyA = false;
        private static bool KeyS = false;
        private static bool KeyD = false;


  

        private static float Shininess = 50;
        private static int counter = 0;
        private static bool gameOver = false;


        private static bool DrawWireFrameOnly = false;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string TextureUniformVariableName = "uTexture";

        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Gyümölcs vadászat";
            windowOptions.Size = new Vector2D<int>(1000, 1000);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }

        private static void Window_Load()
        {
            //Console.WriteLine("Load");

            // set up input handling
            inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
                keyboard.KeyUp += Keyboard_KeyUp;
            }

            Gl = window.CreateOpenGL();

            controller = new ImGuiController(Gl, window, inputContext);

            // Handle resizes
            window.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                Gl.Viewport(s);
            };

            for (int i = 0; i < 600; i++)
            {
                appleArrangementModels.Add(new AppleArrangementModel());
            }

            Gl.ClearColor(System.Drawing.Color.White);

            SetUpObjects();

            LinkProgram();

            //Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
        }

        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, ReadShader("VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, ReadShader("FragmentShader.frag"));
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
        }

        private static string ReadShader(string shaderFileName)
        {
            using (Stream shaderStream = typeof(Program).Assembly.GetManifestResourceStream("GrafikaProjekt.Shaders." + shaderFileName))
            using (StreamReader shaderReader = new StreamReader(shaderStream))
                return shaderReader.ReadToEnd();
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            float speedmultiplier = 1f;
            switch (key)
            {
                case Key.Space:
                    cubeArrangementModel.AnimationEnabeld = !cubeArrangementModel.AnimationEnabeld;
                    break;
               case Key.A:
                    /*speedmultiplier = keyboard.IsKeyPressed(Key.ShiftRight) ? 10.5f : 1.0f;
                    squirrelArrangementModel.Speed = speedmultiplier;
                    squirrelArrangementModel.MoveLeft(3.0);*/
                    KeyA = true;
                    break;
                case Key.D:
                   /* speedmultiplier = keyboard.IsKeyPressed(Key.ShiftRight) ? 10.5f : 1.0f;
                    squirrelArrangementModel.Speed = speedmultiplier;
                    squirrelArrangementModel.MoveRight(3.0);*/
                   KeyD = true;
                    break;
                case Key.W:
                    KeyW = true;                    
                    break;
                case Key.S:
                    KeyS = true ;
                    break;
            }
        }
        private static void Keyboard_KeyUp(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.A:
                    KeyA = false;
                    break;
                case Key.D:
                   KeyD = false;
                    break;
                    case Key.W:
                    KeyW = false;
                    break;
                    case Key.S:
                    KeyS = false;
                    break;
              case Key.Escape:
                    window.Close();
                    break;
                case Key.Space:
                    cameraDescriptor.ToggleCameraMode();
                    break;
            }
        }

        private static void HandleMovement(double deltaTime)
        {
            Vector3D<float> cameraRight = cameraDescriptor.GetCameraRight();
            if (KeyA)
            {
                squirrelArrangementModel.MoveLeft(7.0);
            }
            if (KeyD)
            {
                squirrelArrangementModel.MoveRight(7.0);
            }
            Vector2 movementDirection = new Vector2(0, 0);
            Vector3 squirrelPosition = squirrelArrangementModel.Position;
            if (KeyA)
            {
                movementDirection.X = -1;
            }
            if (KeyD)
            {
                movementDirection.X = 1;
            }
            if (KeyW)
            {
                movementDirection.Y = 1;
            }
            if (KeyS)
            {
                movementDirection.Y = -1;
            }
            cameraDescriptor.Update(new Vector3D<float>(squirrelPosition.X, squirrelPosition.Z, squirrelPosition.Y),movementDirection,(float)deltaTime);

        }

        private static void Window_Update(double deltaTime)
        {
            if (!gameOver)
            {
                HandleMovement(deltaTime);
                foreach (var apples in appleArrangementModels)
                {
                    apples.Move();
                    if (!apples.IsEaten && squirrelArrangementModel.CollidesWith(apples))
                    {

                        apples.IsEaten = true;
                        counter++;
                    }
                    if (counter >= 1000)
                    {
                        gameOver = true;
                        break;
                    }

                }
            }

            controller.Update((float)deltaTime);
        }

        private static unsafe void Window_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);
            Gl.Disable(EnableCap.CullFace); // Add this line
            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();

            SetLightColor();
            SetLightPosition();
            SetViewerPosition();
            SetShininess();
            
            DrawSkyBox();
            DrawSquirrel();

            foreach (var apples in appleArrangementModels)
            {
                if (!apples.IsEaten)
                {
                    DrawApple(apples);
                }
            }

            //ImGuiNET.ImGui.ShowDemoWindow();
            ImGuiNET.ImGui.Begin("Game Info", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            ImGuiNET.ImGui.Text($"Apples eaten: {counter}");
            ImGuiNET.ImGui.SliderFloat("Shininess", ref Shininess, 1, 200);
            ImGuiNET.ImGui.End();



            controller.Render();
        }

        private static unsafe void DrawSkyBox()
        {
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(400f);
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(skyBox.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, skyBox.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, skyBox.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void DrawSquirrel()
        {
            var modelMatrixForCenterCube = Matrix4X4.CreateScale((float)squirrelArrangementModel.Scale);
            Matrix4X4<float> translation = Matrix4X4.CreateTranslation((float)squirrelArrangementModel.X, (float)squirrelArrangementModel.Y, (float)squirrelArrangementModel.Z);
            Matrix4X4<float> rotz = Matrix4X4.CreateRotationX(-(float)Math.PI / 2);
            Matrix4X4<float> modelMatrix = translation * modelMatrixForCenterCube * rotz;
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(squirrel.Vao);
            Gl.DrawElements(GLEnum.Triangles, squirrel.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }

        private static unsafe void DrawApple(AppleArrangementModel appleArrangementModel)
        {
            var modelMatrixForCenterCube = Matrix4X4.CreateScale((float)appleArrangementModel.Scale);
            Matrix4X4<float> translation = Matrix4X4.CreateTranslation((float)appleArrangementModel.X, (float)appleArrangementModel.Y, (float)appleArrangementModel.Z);
            Matrix4X4<float> modelMatrix = translation * modelMatrixForCenterCube;
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(apple.Vao);
            Gl.DrawElements(GLEnum.Triangles, apple.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }


       

        private static unsafe void SetLightColor()
        {
            int location = Gl.GetUniformLocation(program, LightColorVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightColorVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 1f, 1f, 1f);
            CheckError();
        }

        private static unsafe void SetLightPosition()
        {
            int location = Gl.GetUniformLocation(program, LightPositionVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightPositionVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 5f, 1f, 0f);
            //Gl.Uniform3(location, cameraDescriptor.Position.X, cameraDescriptor.Position.Y, cameraDescriptor.Position.Z);
            CheckError();
        }

        private static unsafe void SetViewerPosition()
        {
            int location = Gl.GetUniformLocation(program, ViewPosVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewPosVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, cameraDescriptor.Position.X, cameraDescriptor.Position.Y, cameraDescriptor.Position.Z);
            CheckError();
        }

        private static unsafe void SetShininess()
        {
            int location = Gl.GetUniformLocation(program, ShininessVariableName);

            if (location == -1)
            {
                throw new Exception($"{ShininessVariableName} uniform not found on shader.");
            }

            Gl.Uniform1(location, Shininess);
            CheckError();
        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();

            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
            location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUpObjects()
        {

            float[] face1Color = [1f, 0f, 0f, 1.0f];
            float[] face2Color = [0.0f, 1.0f, 0.0f, 1.0f];
            float[] face3Color = [0.0f, 0.0f, 1.0f, 1.0f];
            float[] face4Color = [1.0f, 0.0f, 1.0f, 1.0f];
            float[] face5Color = [0.0f, 1.0f, 1.0f, 1.0f];
            float[] face6Color = [1.0f, 1.0f, 0.0f, 1.0f];

            //teapot = ObjResourceReader.CreateTeapotWithColor(Gl, face1Color);

            float[] tableColor = [System.Drawing.Color.Azure.R/256f,
                                  System.Drawing.Color.Azure.G/256f,
                                  System.Drawing.Color.Azure.B/256f,
                                  1f];
            table = GlCube.CreateSquare(Gl, tableColor);

            glCubeRotating = GlCube.CreateCubeWithFaceColors(Gl, face1Color, face2Color, face3Color, face4Color, face5Color, face6Color);

            skyBox = GlCube.CreateInteriorCube(Gl, "");

            squirrel = ObjResourceReader.Create3DObjectWithColor(Gl, [0.6f, 0.3f, 0.0f, 1.0f], "squirrel");

            for (int i = 0; i < 600; i++)
            {
                apple = ObjResourceReader.Create3DObjectWithColor(Gl, new float[] { 1.0f, 0.0f, 0.0f, 1.0f }, "apple");
                appleArrangementModels.Add(new AppleArrangementModel { GlObject = apple });
            }
            //Vector3 initialCameraPosition = squirrelArrangementModel.Position - new Vector3(0, 4, 0);
            //cameraDescriptor.SetPosition(initialCameraPosition);

            glSphere = GlObject.CreateSphere(5.0f, Gl);

            glPlate = GlObject.CreateChalice(Gl);
        }

        

        private static void Window_Closing()
        {
            squirrel.ReleaseGlObject();
            apple.ReleaseGlObject();
            glCubeRotating.ReleaseGlObject();
        }

        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 1000);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }


         private static unsafe void SetViewMatrix()
         {
             var viewMatrix = Matrix4X4.CreateLookAt(cameraDescriptor.Position, cameraDescriptor.Target, cameraDescriptor.UpVector);
             int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

             if (location == -1)
             {
                 throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
             }

             Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
             CheckError();
         }
        public static void CheckError()
        {
            var error = (Silk.NET.OpenGL.ErrorCode)Gl.GetError();
            if (error != Silk.NET.OpenGL.ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}