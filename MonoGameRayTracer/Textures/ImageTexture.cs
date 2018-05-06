using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MonoGameRayTracer.Textures
{
    using MGTexture = Microsoft.Xna.Framework.Graphics.Texture2D;

    public class ImageTexture : Texture
    {
        private byte[] m_Data;
        private int m_Width;
        public int m_Height;

        public ImageTexture(MGTexture texture)
        {
            var data = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(data);

            var list = new List<byte>();
            for (var i = 0; i < texture.Width * texture.Height; i++)
            {
                list.Add(data[i].R);
                list.Add(data[i].G);
                list.Add(data[i].B);
            }

            m_Data = list.ToArray();
            m_Width = texture.Width;
            m_Height = texture.Height;
        }

        public ImageTexture(ref byte[] data, int width, int height)
        {
            m_Data = data;
            m_Width = width;
            m_Height = height;
        }

        public override Vector3 Tex2D(ref float u, ref float v, ref Vector3 vector)
        {
            var i = (int)(u * m_Width);
            var j = (int)((1 - v) * m_Height - 0.001f);

            if (i < 0)
                i = 0;

            if (j < 0)
                j = 0;

            if (i > m_Width - 1)
                i = m_Width - 1;

            if (j > m_Height - 1)
                j = m_Height - 1;

            var r = m_Data[3 * i + 3 * m_Width * j] / 255.0f;
            var g = m_Data[3 * i + 3 * m_Width * j + 1] / 255.0f;
            var b = m_Data[3 * i + 3 * m_Width * j + 2] / 255.0f;

            return new Vector3(r, g, b);
        }
    }
}
