using Microsoft.Xna.Framework;
using MonoGameRayTracer.Utils;

namespace MonoGameRayTracer.Materials
{
    public class LambertMaterial : Material
    {
        private Vector3 m_Albedo;

        public Vector3 Albedo => m_Albedo;

        public LambertMaterial(Vector3 albedo)
        {
            m_Albedo = albedo;
        }

        public LambertMaterial(float x, float y, float z)
        {
            m_Albedo = new Vector3(x, y, z);
        }

        public override bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered)
        {
            var target = record.P + record.Normal + Mathf.RandomInUnitySphere();
            scattered = new Ray(record.P, target - record.P);
            attenuation = m_Albedo;
            return true;
        }
    }
}
