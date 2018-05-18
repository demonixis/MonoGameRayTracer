using Microsoft.Xna.Framework;
using MonoGameRayTracer.Textures;
using MonoGameRayTracer.Utils;

namespace MonoGameRayTracer.Materials
{
    public class LambertMaterial : Material
    {
        private Texture m_EmissiveTexture;
        private bool m_EmissiveEnabled;

        public LambertMaterial(Texture texture, Texture emissive = null)
        {
            m_Texture = texture;
            m_TextureEnabled = true;
            m_EmissiveTexture = emissive;
            m_EmissiveEnabled = m_EmissiveTexture != null;
        }

        public LambertMaterial(float x, float y, float z)
        {
            m_Albedo = new Vector3(x, y, z);
        }

        public override bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered)
        {
            var target = record.P + record.Normal + Mathf.RandomInUnitySphere();
            var direction = target - record.P;
            scattered.Set(ref record.P, ref direction);
            attenuation = m_TextureEnabled ? m_Texture.Tex2D(ref record.U, ref record.V, ref record.P) : m_Albedo;
            return true;
        }

        public override Vector3 Emitted(ref float u, ref float v, ref Vector3 p)
        {
            return m_EmissiveEnabled ? m_EmissiveTexture.Tex2D(ref u, ref v, ref p) : Vector3.Zero;
        }
    }
}
