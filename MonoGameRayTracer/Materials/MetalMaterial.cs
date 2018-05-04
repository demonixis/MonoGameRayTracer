using Microsoft.Xna.Framework;
using MonoGameRayTracer.Utils;

namespace MonoGameRayTracer.Materials
{
    public class MetalMaterial : Material
    {
        private Vector3 m_Albedo;
        private float m_Fuzz;

        public Vector3 Albedo => m_Albedo;
        public float Fuzz => m_Fuzz;

        public override Vector4 Value => new Vector4(m_Albedo.X, m_Albedo.Y, m_Albedo.Z, 1);

        public MetalMaterial(float x, float y, float z, float fuzz)
        {
            m_Albedo = new Vector3(x, y, z);

            if (fuzz < 1)
                m_Fuzz = fuzz;
            else
                m_Fuzz = 1;
        }

        public override bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered)
        {
            var reflected = Vector3.Reflect(Mathf.UnitVector(ray.Direction), record.Normal);
            scattered = new Ray(record.P, reflected + m_Fuzz * Mathf.RandomInUnitySphere());
            attenuation = m_Albedo;
            return Vector3.Dot(scattered.Direction, record.Normal) > 0;
        }
    }
}
