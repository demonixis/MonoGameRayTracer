using Microsoft.Xna.Framework;
using MonoGameRayTracer.Textures;
using MonoGameRayTracer.Utils;

namespace MonoGameRayTracer.Materials
{
    public class LambertMaterial : Material
    {
        private Vector3 m_Albedo;
        private Texture m_Texture;

        public Vector3 Albedo => m_Albedo;

        public LambertMaterial(Vector3 albedo)
        {
            m_Albedo = albedo;
        }

        public LambertMaterial(Texture texture)
        {
            m_Texture = texture;
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

            if (m_Texture != null)
                attenuation = m_Texture.Tex2D(ref record.U, ref record.V, ref record.P);
            else
                attenuation = m_Albedo;

            return true;
        }
    }
}
