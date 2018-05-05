using Microsoft.Xna.Framework;
using MonoGameRayTracer.Utils;

namespace MonoGameRayTracer.Textures
{
    public class CheckerTexture : Texture
    {
        private Texture m_OddTexture;
        private Texture m_EvenTexture;

        public CheckerTexture()
        {
            m_EvenTexture = new ConstantTexture(new Vector3(0.3f, 0.3f, 0.3f));
            m_OddTexture = new ConstantTexture(new Vector3(0.9f, 0.9f, 0.9f));
        }

        public CheckerTexture(Texture odd, Texture even)
        {
            m_OddTexture = odd;
            m_EvenTexture = even;
        }

        public override Vector3 Tex2D(ref float u, ref float v, ref Vector3 vector)
        {
            var sines = Mathf.Sin(10.0f * vector.X) * Mathf.Sin(10.0f * vector.Y) * Mathf.Sin(10.0f * vector.Z);

            if (sines < 0)
                return m_OddTexture.Tex2D(ref u, ref v, ref vector);

            return m_EvenTexture.Tex2D(ref u, ref v, ref vector);
        }

        public override void Tex2D(ref HitRecord record, ref Vector3 result)
        {
            var vector = record.P;
            var sines = Mathf.Sin(10.0f * vector.X) * Mathf.Sin(10.0f * vector.Y) * Mathf.Sin(10.0f * vector.Z);

            if (sines < 0)
                result = m_OddTexture.Tex2D(ref record.U, ref record.V, ref vector);

            result = m_EvenTexture.Tex2D(ref record.U, ref record.V, ref vector);
        }
    }
}
