using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace ShaderRenderer
{
    public class Texture
    {
        public int id;
        public int w;
        public int h;
        public string name;
        public string imgPath;
        private void GenID(string path)
        {
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            Bitmap bmp = new Bitmap(path);

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, Program.repeatTex ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, Program.repeatTex ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.Clamp);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            w = bmp.Width;
            h = bmp.Height;
        }
        private void GenID(Bitmap bmp)
        {
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, Program.repeatTex ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, Program.repeatTex ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.Clamp);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            w = bmp.Width;
            h = bmp.Height;
        }
        public Texture(string path = "")
        {
            if (File.Exists(path))
            {
                imgPath = path;
                GenID(path);
                name = Path.GetFileNameWithoutExtension(path);
            }
                
        }
        public Texture(int width, int height, Color4 color)
        {
            Bitmap bmp = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmp.SetPixel(x, y, ((Color)color));
                }
            }
            GenID(bmp);
        }
        public Texture(int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmp.SetPixel(x, y, Color.White);
                }
            }
            GenID(bmp);
        }
        public Texture()
        {
            Bitmap bmp = new Bitmap(1, 1);
            bmp.SetPixel(0, 0, Color.White);
            GenID(bmp);
        }
        public Texture(Color4 color)
        {
            Bitmap bmp = new Bitmap(1, 1);
            bmp.SetPixel(0, 0, ((Color)color));
            GenID(bmp);
        }
        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, id);
        }
    }
}
