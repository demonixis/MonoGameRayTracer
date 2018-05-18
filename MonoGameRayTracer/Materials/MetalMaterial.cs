using Microsoft.Xna.Framework;
using MonoGameRayTracer.Textures;
using MonoGameRayTracer.Utils;

namespace MonoGameRayTracer.Materials
{
    public class MetalMaterial : Material
    {
        private float m_Fuzz;

        public float Fuzz => m_Fuzz;

        public MetalMaterial(float x, float y, float z, float fuzz)
        {
            m_Albedo = new Vector3(x, y, z);

            if (fuzz < 1)
                m_Fuzz = fuzz;
            else
                m_Fuzz = 1;
        }

        public MetalMaterial(Texture texture, float fuzz)
        {
            m_Texture = texture;
            m_TextureEnabled = true;

            if (fuzz < 1)
                m_Fuzz = fuzz;
            else
                m_Fuzz = 1;
        }

        public override bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered)
        {
            var reflected = Vector3.Reflect(Mathf.UnitVector(ray.Direction), record.Normal);
            var direction = reflected + m_Fuzz * Mathf.RandomInUnitySphere();
            scattered.Set(ref record.P, ref direction);
            attenuation = m_TextureEnabled ? m_Texture.Tex2D(ref record.U, ref record.V, ref record.P) : m_Albedo;
            return Vector3.Dot(scattered.Direction, record.Normal) > 0;
        }
    }
}
