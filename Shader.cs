using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ShaderRenderer
{
    public class Shader
    {
        public int HANDLE = -111;

        public Shader(string vertexPath, string fragmentPath)
        {
            string vertexSource = File.ReadAllText(vertexPath);
            string fragmentSource = File.ReadAllText(fragmentPath);

            CompileShader(vertexSource, fragmentSource);
        }
        public void CompileShader(string vertexSource, string fragmentSource)
        {

            int VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, vertexSource);

            int FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, fragmentSource);
            Program.logText = "";

            GL.CompileShader(VertexShader);
            string infoLogVertex = GL.GetShaderInfoLog(VertexShader);
            Program.logText += "Vertex Shader Error: " + infoLogVertex + "\n";

            GL.CompileShader(FragmentShader);
            string infoLogFragment = GL.GetShaderInfoLog(FragmentShader);
            Program.logText += "Fragment Shader Error: " + infoLogFragment + "\n";
            if(HANDLE == -111)
                HANDLE = GL.CreateProgram();

            GL.AttachShader(HANDLE, VertexShader);
            GL.AttachShader(HANDLE, FragmentShader);

            GL.LinkProgram(HANDLE);

            GL.DetachShader(HANDLE, VertexShader);
            GL.DetachShader(HANDLE, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }

        public void Use()
        {
            GL.UseProgram(HANDLE);
        }

        public void SetUniform(string name, float value)
        {
            GL.Uniform1(GL.GetUniformLocation(HANDLE, name), value);
        }
        public void SetUniform(string name, double value)
        {
            GL.Uniform1(GL.GetUniformLocation(HANDLE, name), value);
        }
        public void SetUniform(string name, int value)
        {
            GL.Uniform1(GL.GetUniformLocation(HANDLE, name), value);
        }
        public void SetUniform(string name, Vector2 value)
        {
            GL.Uniform2(GL.GetUniformLocation(HANDLE, name), value);
        }
        public void SetUniform(string name, Vector3 value)
        {
            GL.Uniform3(GL.GetUniformLocation(HANDLE, name), value);
        }
        public void SetUniform(string name, Vector4 value)
        {
            GL.Uniform4(GL.GetUniformLocation(HANDLE, name), value);
        }
        public void SetUniform(string name, Color4 value)
        {
            GL.Uniform4(GL.GetUniformLocation(HANDLE, name), value);
        }
        public void SetUniform(string name, ref Matrix4 value, bool transpose = true)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(HANDLE, name), transpose, ref value);
        }
        public void SetUniform(string name, ref Matrix3 value, bool transpose = true)
        {
            GL.UniformMatrix3(GL.GetUniformLocation(HANDLE, name), transpose, ref value);
        }
        public void SetUniform(string name, ref Matrix2 value, bool transpose = true)
        {
            GL.UniformMatrix2(GL.GetUniformLocation(HANDLE, name), transpose, ref value);
        }

        // Dispose Shit
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(HANDLE);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            GL.DeleteProgram(HANDLE);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
