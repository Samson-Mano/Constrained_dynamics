// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPBD_soft_body_dynamics.src.opentk_control.opentk_buffer
{
    public class TextureBuffer
    {
        private int _rendererID;

        // private string _textureFilepath;

        private int _textureWidth;
        private int _textureHeight;
        private int _textureBpp;

        private byte[] _localBuffer;


        public int TextureID { get { return _rendererID; } }


        public TextureBuffer(byte[] res_pic)
        {

            using (MemoryStream ms = new MemoryStream(res_pic))
            {
                ImageResult image = ImageResult.FromStream(ms, ColorComponents.RedGreenBlueAlpha);

                _textureWidth = image.Width;
                _textureHeight = image.Height;
                _textureBpp = 4;

                _localBuffer = image.Data;
            }


            // Generate texture
            _rendererID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _rendererID);

            // Texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Upload texture
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba8,
                _textureWidth,
                _textureHeight,
                0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Rgba,
                PixelType.UnsignedByte,
                _localBuffer);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Bind(int slot = 0)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.BindTexture(TextureTarget.Texture2D, _rendererID);
        }

        public void UnBind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Delete()
        {
            GL.DeleteTexture(_rendererID);
        }



    }
}
