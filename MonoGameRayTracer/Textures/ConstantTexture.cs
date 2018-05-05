using Microsoft.Xna.Framework;

namespace MonoGameRayTracer.Textures
{
    public class ConstantTexture : Texture
    {
        private Vector3 m_Color;

        public ConstantTexture(Vector3 color) => m_Color = color;
        public ConstantTexture(Color color) => m_Color = color.ToVector3();

        public override Vector3 Tex2D(ref float u, ref float v, ref Vector3 vector) => m_Color;
        public override void Tex2D(ref HitRecord record, ref Vector3 result) => result = m_Color;
    }
}
