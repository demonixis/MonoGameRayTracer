using Microsoft.Xna.Framework;
using MonoGameRayTracer.Textures;
using MonoGameRayTracer.Utils;

namespace MonoGameRayTracer.Materials
{
    public class DieletricMaterial : Material
    {
        private float m_RefIdx;

        public DieletricMaterial(float refIdx, Texture texture = null)
        {
            m_RefIdx = refIdx;
            m_Albedo = new Vector3(1.0f, 1.0f, 0.0f);
            m_Texture = texture;
            m_TextureEnabled = texture != null;
        }

        public DieletricMaterial(float refIdx, Vector3 albedo)
        {
            m_RefIdx = refIdx;
            m_Albedo = albedo;
        }

        public override bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered)
        {
            var rayDirection = ray.Direction;
            var outwardNormal = Vector3.Zero;
            var reflected = Vector3.Reflect(rayDirection, record.Normal);
            var niOverNt = 0.0f;
            var refracted = Vector3.Zero;
            var reflectProbe = 0.0f;
            var cosine = 0.0f;

            attenuation = m_TextureEnabled ? m_Texture.Tex2D(ref record.U, ref record.V, ref record.P) : m_Albedo;

            if (Vector3.Dot(rayDirection, record.Normal) > 0)
            {
                outwardNormal = -record.Normal;
                niOverNt = m_RefIdx;
                cosine = m_RefIdx * Vector3.Dot(rayDirection, record.Normal) / rayDirection.Length();
            }
            else
            {
                outwardNormal = record.Normal;
                niOverNt = 1.0f / m_RefIdx;
                cosine = -Vector3.Dot(rayDirection, record.Normal) / rayDirection.Length();
            }

            var direction = rayDirection;
            if (Mathf.Refract(ref direction, ref outwardNormal, niOverNt, ref refracted))
            {
                reflectProbe = Mathf.Schlick(cosine, m_RefIdx);
            }
            else
            {
                scattered.Set(ref record.P, ref refracted);
                reflectProbe = 1.0f;
            }

            if (Random.Value < reflectProbe)
                scattered.Set(ref record.P, ref reflected);
            else
                scattered.Set(ref record.P, ref refracted);

            return true;
        }
    }
}
