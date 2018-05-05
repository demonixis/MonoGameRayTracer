using System.Numerics;

namespace MonoGameRayTracer.Textures
{
    using XnaColor = Microsoft.Xna.Framework.Color;
    public class ConstantTexture : Texture
    {
        private Vector3 m_Color;

        public ConstantTexture(Vector3 color) => m_Color = color;
        public ConstantTexture(XnaColor color) => m_Color = new Vector3(color.R, color.G, color.B);

        public override Vector3 Tex2D(ref float u, ref float v, ref Vector3 vector) => m_Color;
        public override void Tex2D(ref HitRecord record, ref Vector3 result) => result = m_Color;
    }
}
