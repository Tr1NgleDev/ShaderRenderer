using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using ImGuiA;
using System.Windows.Forms;

namespace ShaderRenderer
{
    public class Window : GameWindow
    {
        public Shader shader;

        int VBO;
        int VAO;
        public string vertShader;
        public string fragShader;
        private float fps;
        float time;
        public Window(int w, int h, float fps, string vertShaderFile, string fragmentShaderFile) : base(w, h, GraphicsMode.Default, "Shader Renderer")
        {
            this.vertShader = vertShaderFile;
            this.fragShader = fragmentShaderFile;
            this.fps = fps;
        }
        public void Start()
        {
            Run(fps, fps);
        }
        protected override void OnFileDrop(FileDropEventArgs e)
        {
            base.OnFileDrop(e);
            if(e.FileName.Contains(".shader") || e.FileName.Contains(".glsl") || e.FileName.Contains(".sh") || e.FileName.Contains(".fsh"))
            {
                fragShader = e.FileName;
                LoadShader(false);
            }
            else if(e.FileName.Contains(".png") || e.FileName.Contains(".jpg"))
            {
                AddTexture(e.FileName);
            }
        }
        public void AddTexture(string path)
        {
            textures.Add(new Texture(path));
            if(textures.Count > 32)
                textures.RemoveAt(32);
        }
        protected override void OnUnload(EventArgs e)
        {
            shader.Dispose();
            base.OnUnload(e);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            shader = new Shader(vertShader, fragShader);
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }
        public ImGuiController imgui;
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
            imgui.WindowResized(Width, Height);
        }
        float[] vertices = new float[]
        {
            -1f, -1f, 0f,   0f, 1f,
            1f, -1f, 0f,   1f, 1f,
            1f, 1f, 0f,   1f, 0f,
            -1f, 1f, 0f,   0f, 0f,
        };
        List<Texture> textures = new List<Texture>();
        public Action<float> DrawGUI;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            imgui.Update(this, (float)e.Time);
            time += (float)e.Time * 1000f;
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            shader.Use();
            foreach (var tex in textures)
            {
                shader.SetUniform(tex.name, textures.IndexOf(tex));
                tex.Use(TextureUnit.Texture0 + textures.IndexOf(tex));
            }
            Program.time = time;
            GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length / 5);
            if (DrawGUI != null)
                DrawGUI.Invoke((float)e.Time);
            imgui.Render();
            Context.SwapBuffers();
        }
        public void UpdateTextures()
        {
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i] = new Texture(textures[i].imgPath);
            }
        }
        public void LoadShader(bool saveUniforms)
        {
            List<Uniform> uniformsToSave = new List<Uniform>();

            foreach (var item in Program.uniforms)
            {
                uniformsToSave.Add(new Uniform() { name = item.name, value = item.value, loc = item.loc });
            }

            time = 0f;
            shader.CompileShader(File.ReadAllText(vertShader), File.ReadAllText(fragShader));
            if (saveUniforms)
                foreach (var item in uniformsToSave)
                    shader.SetUniform(item.name, item.value);
            else
                Program.uniformsIsTime = new bool[0];
            
        }
        public void RestartTime()
        {
            time = 0f;
        }
        public Uniform[] GetShaderUniforms()
        {
            try
            {
                string[] shaderCode = File.ReadAllLines(fragShader);
                List<Uniform> uniforms = new List<Uniform>();
                for (int i = 0; i < shaderCode.Length; i++)
                {
                    string line = shaderCode[i];
                    if (line.StartsWith("uniform "))
                    {
                        var cdb = line.Remove(line.Length - 1, 1).Split(' ');
                        switch (cdb[1])
                        {
                            case "float":
                                if (GL.GetUniformLocation(shader.HANDLE, cdb[2]) != -1)
                                {
                                    GL.GetUniform(shader.HANDLE, GL.GetUniformLocation(shader.HANDLE, cdb[2]), out float valf);
                                    uniforms.Add(new Uniform() { name = cdb[2], value = valf, loc = GL.GetUniformLocation(shader.HANDLE, cdb[2]) });
                                }
                                break;
                            case "int":
                                if (GL.GetUniformLocation(shader.HANDLE, cdb[2]) != -1)
                                {
                                    GL.GetUniform(shader.HANDLE, GL.GetUniformLocation(shader.HANDLE, cdb[2]), out int vali);
                                    uniforms.Add(new Uniform() { name = cdb[2], value = vali, loc = GL.GetUniformLocation(shader.HANDLE, cdb[2]) });
                                }
                                break;
                            case "bool":
                                if (GL.GetUniformLocation(shader.HANDLE, cdb[2]) != -1)
                                {
                                    GL.GetUniform(shader.HANDLE, GL.GetUniformLocation(shader.HANDLE, cdb[2]), out int valb);
                                    uniforms.Add(new Uniform() { name = cdb[2], value = valb == 1, loc = GL.GetUniformLocation(shader.HANDLE, cdb[2]) });
                                }
                                break;
                            case "vec2":
                                float[] valf2 = new float[2];
                                if (GL.GetUniformLocation(shader.HANDLE, cdb[2]) != -1)
                                {
                                    GL.GetUniform(shader.HANDLE, GL.GetUniformLocation(shader.HANDLE, cdb[2]), valf2);
                                    uniforms.Add(new Uniform() { name = cdb[2], value = new Vector2(valf2[0], valf2[1]), loc = GL.GetUniformLocation(shader.HANDLE, cdb[2]) });
                                }
                                break;
                            case "vec3":
                                float[] valf3 = new float[3];
                                if (GL.GetUniformLocation(shader.HANDLE, cdb[2]) != -1)
                                {
                                    GL.GetUniform(shader.HANDLE, GL.GetUniformLocation(shader.HANDLE, cdb[2]), valf3);
                                    uniforms.Add(new Uniform() { name = cdb[2], value = new Vector3(valf3[0], valf3[1], valf3[2]), loc = GL.GetUniformLocation(shader.HANDLE, cdb[2]) });
                                }
                                break;
                            case "vec4":
                                float[] valf4 = new float[4];
                                if(GL.GetUniformLocation(shader.HANDLE, cdb[2]) != -1)
                                {
                                    GL.GetUniform(shader.HANDLE, GL.GetUniformLocation(shader.HANDLE, cdb[2]), valf4);
                                    uniforms.Add(new Uniform() { name = cdb[2], value = new Vector4(valf4[0], valf4[1], valf4[2], valf4[3]), loc = GL.GetUniformLocation(shader.HANDLE, cdb[2]) });
                                }
                                
                                break;
                        }

                    }
                }
                return uniforms.ToArray();
            }
            catch 
            {
                return new Uniform[0];
            }
            
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Close();
        }
    }
    public struct Uniform
    {
        public string name;
        public dynamic value;
        public int loc;
    }
    
    internal class Program
    {
        public static float time;
        public static Window window;
        public static ImGuiController imgui;
        public static Uniform[] uniforms = new Uniform[0];
        private static int a = 0;
        public static bool[] uniformsIsTime = new bool[0];
        public static bool showedStartupWindow = false;
        public static string logText = "";
        public static bool repeatTex = false;
        public static bool snapUniformValues = false;
        public static float maxVal = 50;
        public static float minVal = -50;

        public static float Step(float edge, float x)
        {
            return x < edge ? 0f : 1f;
        }
        public static bool InRange(float v, float bottom, float top)
        {
            return Step(bottom, v) - Step(top, v) == 1f;
        }
        static void OpenShader()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.CurrentDirectory;
                openFileDialog.Filter = "Shader Files (*.glsl; *.frag; *.fragment; *.shader; *.fsh; *.sh)|*.glsl; *.frag; *.fragment; *.shader; *.fsh; *.sh|All files (*.*)|*.*";

                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    window.AddTexture(openFileDialog.FileName);
            }
        }
        static void AddTexture()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.CurrentDirectory;
                openFileDialog.Filter = "Image Files (*.png; *.jpg; *.bmp)|*.png; *.jpg; *.bmp|All files (*.*)|*.*";

                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    window.AddTexture(openFileDialog.FileName);
            }
        }
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            window = new Window(1280, 720, 120, "vertexShader.glsl", "fragmentShader.glsl");
            window.DrawGUI = (float deltaTime) =>
            {
                try
                {
                    uniforms = window.GetShaderUniforms();
                    if (uniformsIsTime.Length != uniforms.Length)
                    {
                        uniformsIsTime = new bool[uniforms.Length];
                    }
                    if (ImGui.BeginMainMenuBar())
                    {
                        if (ImGui.BeginMenu("Files"))
                        {
                            if (ImGui.MenuItem("Open Shader", "CTRL+O"))
                            {
                                OpenShader();
                            }
                            if (ImGui.MenuItem("Add Texture", "CTRL+SHIFT+O"))
                            {
                                AddTexture();
                            }
                            ImGui.EndMenu();
                        }
                        if (ImGui.BeginMenu("Options"))
                        {
                            if (ImGui.MenuItem("Reload Shader", "CTRL+R"))
                            {
                                window.LoadShader(true);
                            }
                            if (ImGui.MenuItem("Restart Shader Time", "CTRL+T"))
                            {
                                window.RestartTime();
                            }
                            ImGui.Checkbox("Repeat Texture", ref repeatTex);
                            if (ImGui.MenuItem("Update Textures", ""))
                            {
                                window.UpdateTextures();
                            }
                            ImGui.Checkbox("Snap Uniform Values", ref snapUniformValues);

                            ImGui.SliderFloat("Max Slider Value", ref maxVal, 0, 100);
                            if (InRange(Mathf.Round(maxVal), maxVal - 0.4999f, maxVal + 0.4999f))
                                maxVal = Mathf.Round(maxVal);

                            ImGui.SliderFloat("Min Slider Value", ref minVal, -100, 0);
                            if (InRange(Mathf.Round(minVal), minVal - 0.4999f, minVal + 0.4999f))
                                minVal = Mathf.Round(minVal);
                            ImGui.EndMenu();
                        }
                        ImGui.Text("            FPS:" + Mathf.Round(1f / deltaTime));
                        ImGui.EndMainMenuBar();
                    }

                    if (ImGui.Begin("Uniforms"))
                    {
                        List<string> uniformsStringArr = new List<string>();
                        foreach (var item in uniforms)
                        {
                            uniformsStringArr.Add(item.name + " = " + item.value);
                        }
                        ImGui.ListBox("", ref a, uniformsStringArr.ToArray(), uniformsStringArr.Count);
                        for (int i = 0; i < uniforms.Length; i++)
                        {
                            Uniform item = uniforms[i];

                            if (item.value is float)
                            {
                                bool isTime = uniformsIsTime[i];
                                ImGui.Checkbox(item.name + " is Time", ref isTime);
                                uniformsIsTime[i] = isTime;
                                float v = item.value;
                                if (ImGui.SliderFloat(item.name, ref v, minVal, maxVal) && snapUniformValues)
                                {
                                    if (InRange(Mathf.Round(v), v - 0.4999f, v + 0.4999f))
                                        v = Mathf.Round(v);
                                }
                                window.shader.SetUniform(item.name, v);
                            }
                            if (item.value is int)
                            {
                                bool isTime = uniformsIsTime[i];
                                ImGui.Checkbox(item.name + " is Time", ref isTime);
                                uniformsIsTime[i] = isTime;
                                int v = item.value;
                                ImGui.SliderInt(item.name, ref v, (int)minVal, (int)maxVal);
                                window.shader.SetUniform(item.name, v);
                            }
                            if (item.value is bool)
                            {
                                bool v = item.value;
                                ImGui.Checkbox(item.name, ref v);
                                window.shader.SetUniform(item.name, v == true ? 1 : 0);
                            }
                            if (item.value is Vector2)
                            {
                                System.Numerics.Vector2 v = new System.Numerics.Vector2(item.value.X, item.value.Y);
                                if (ImGui.SliderFloat2(item.name, ref v, minVal, maxVal) && snapUniformValues)
                                {
                                    if (InRange(Mathf.Round(v.X), v.X - 0.4999f, v.X + 0.4999f))
                                        v.X = Mathf.Round(v.X);
                                    if (InRange(Mathf.Round(v.Y), v.Y - 0.4999f, v.Y + 0.4999f))
                                        v.Y = Mathf.Round(v.Y);
                                }
                                window.shader.SetUniform(item.name, new Vector2(v.X, v.Y));
                            }
                            if (item.value is Vector3)
                            {
                                System.Numerics.Vector3 v = new System.Numerics.Vector3(item.value.X, item.value.Y, item.value.Z);
                                if (ImGui.SliderFloat3(item.name, ref v, minVal, maxVal) && snapUniformValues)
                                {
                                    if (InRange(Mathf.Round(v.X), v.X - 0.4999f, v.X + 0.4999f))
                                        v.X = Mathf.Round(v.X);
                                    if (InRange(Mathf.Round(v.Y), v.Y - 0.4999f, v.Y + 0.4999f))
                                        v.Y = Mathf.Round(v.Y);
                                    if (InRange(Mathf.Round(v.Z), v.Z - 0.4999f, v.Z + 0.4999f))
                                        v.Z = Mathf.Round(v.Z);
                                }
                                window.shader.SetUniform(item.name, new Vector3(v.X, v.Y, v.Z));
                            }
                            if (item.value is Vector4)
                            {
                                System.Numerics.Vector4 v = new System.Numerics.Vector4(item.value.X, item.value.Y, item.value.Z, item.value.W);
                                if (ImGui.SliderFloat4(item.name, ref v, minVal, maxVal) && snapUniformValues)
                                {
                                    if (InRange(Mathf.Round(v.X), v.X - 0.4999f, v.X + 0.4999f))
                                        v.X = Mathf.Round(v.X);
                                    if (InRange(Mathf.Round(v.Y), v.Y - 0.4999f, v.Y + 0.4999f))
                                        v.Y = Mathf.Round(v.Y);
                                    if (InRange(Mathf.Round(v.Z), v.Z - 0.4999f, v.Z + 0.4999f))
                                        v.Z = Mathf.Round(v.Z);
                                    if (InRange(Mathf.Round(v.W), v.W - 0.4999f, v.W + 0.4999f))
                                        v.W = Mathf.Round(v.W);
                                }
                                window.shader.SetUniform(item.name, new Vector4(v.X, v.Y, v.Z, v.W));
                            }

                        }

                        ImGui.End();
                    }
                    for (int i = 0; i < uniformsIsTime.Length; i++)
                    {
                        if (uniformsIsTime[i])
                        {
                            window.shader.SetUniform(uniforms[i].name, time);
                        }
                    }
                    if (!showedStartupWindow)
                        if (ImGui.Begin("Shader Renderer Info"))
                        {
                            ImGui.SetWindowSize(new System.Numerics.Vector2(400, 213));
                            ImGui.TextWrapped(" Shader Renderer v1.0.0\n\n   Drop images to window to add it as texture uniform.\n   Drop shader file to window to open it.\n-------------------------------------\n   CTRL+R to reload shader\n\n   CTRL+T to restart shader time\n\n\nMade by: Tr1NgleDev");
                            if (ImGui.Button("Close"))
                            {
                                showedStartupWindow = true;
                            }
                            ImGui.End();
                        }
                    if (ImGui.Begin("Log"))
                    {
                        ImGui.Text(logText);
                        ImGui.End();
                    }
                    if (Input.KeyJustPressed(Keys.R) && Input.KeyDown(Keys.LControl))
                        window.LoadShader(true);
                    if (Input.KeyJustPressed(Keys.T) && Input.KeyDown(Keys.LControl))
                        window.RestartTime();
                    if (Input.KeyDown(Keys.LShift) && Input.KeyJustPressed(Keys.O) && Input.KeyDown(Keys.LControl))
                        AddTexture();
                    else if (Input.KeyJustPressed(Keys.O) && Input.KeyDown(Keys.LControl))
                        OpenShader();
                    Input.lastKeyboardState = Keyboard.GetState();
                }
                catch{}
            };

            // ImGui lol
            imgui = new ImGuiController(window.Width, window.Height);
            window.imgui = imgui;

            window.Start();
        }
    }
}
