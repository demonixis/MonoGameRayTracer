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
            scattered = new Ray(record.P, target - record.P);

            if (m_Texture != null)
            {
                var u = 0.0f;
                var v = 0.0f;
                Hitable.GetUV(ref record, ref u, ref v);
                attenuation = m_Texture.Tex2D(u, v, ref record.P);
            }
            else
                attenuation = m_Albedo;

            return true;
        }
    }
}
