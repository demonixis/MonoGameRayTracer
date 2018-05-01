using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGameRayTracer.Utils;

namespace MonoGameRayTracer.Materials
{
    public class DieletricMaterial : Material
    {
        float m_RefIdx;

        public override Vector4 Value => new Vector4(m_RefIdx, 0, 0, 2);

        public DieletricMaterial(float refIdx)
        {
            m_RefIdx = refIdx;
        }

        public override bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered)
        {
            var outwardNormal = Vector3.Zero;
            var reflected = Vector3.Reflect(ray.Direction, record.Normal);
            var niOverNt = 0.0f;

            attenuation = new Vector3(1.0f, 1.0f, 0.0f);
            var refracted = Vector3.Zero;
            var reflectProbe = 0.0f;
            var cosine = 0.0f;

            if (Vector3.Dot(ray.Direction, record.Normal) > 0)
            {
                outwardNormal = -record.Normal;
                niOverNt = m_RefIdx;
                cosine = m_RefIdx * Vector3.Dot(ray.Direction, record.Normal) / ray.Direction.Length();
            }
            else
            {
                outwardNormal = record.Normal;
                niOverNt = 1.0f / m_RefIdx;
                cosine = -Vector3.Dot(ray.Direction, record.Normal) / ray.Direction.Length();
            }

            var direction = ray.Direction;
            if (Mathf.Refract(ref direction, ref outwardNormal, niOverNt, ref refracted))
            {
                reflectProbe = Mathf.Schlick(cosine, m_RefIdx);
            }
            else
            {
                scattered = new Ray(record.P, refracted);
                reflectProbe = 1.0f;
            }

            if (Random.Value < reflectProbe)
                scattered = new Ray(record.P, reflected);
            else
                scattered = new Ray(record.P, refracted);

            return true;
        }
    }
}
