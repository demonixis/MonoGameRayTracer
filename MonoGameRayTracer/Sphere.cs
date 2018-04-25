using Microsoft.Xna.Framework;
using System;

namespace MonoGameRayTracer
{
    public class Sphere : Hitable
    {
        private Vector3 m_Center;
        private float m_Radius;

        public Vector3 Center => m_Center;
        public float Radius => m_Radius;

        public Sphere(Vector3 center, float radius, Material material)
        {
            m_Center = center;
            m_Radius = radius;
            m_Material = material;
        }

        public Sphere(ref Vector3 center, ref float radius, Material material)
        {
            m_Center = center;
            m_Radius = radius;
            m_Material = material;
        }

        public override bool Hit(ref Ray ray, float min, float max, ref HitRecord record)
        {
            var oc = ray.Origin - m_Center;
            var a = Vector3.Dot(ray.Direction, ray.Direction);
            var b = Vector3.Dot(oc, ray.Direction);
            var c = Vector3.Dot(oc, oc) - m_Radius * m_Radius;
            var disciminent = b * b - a * c;

            if (disciminent > 0)
            {
                var temp = (-b - (float)Math.Sqrt(b * b - a * c)) / a;

                if (temp < max && temp > min)
                {
                    record.T = temp;
                    record.P = ray.PointAtParameter(record.T);
                    record.Normal = (record.P - m_Center) / Radius;
                    record.Material = m_Material;
                    return true;
                }

                temp = (-b + (float)Math.Sqrt(b * b - a * c)) / a;

                if (temp < max && temp > min)
                {
                    record.T = temp;
                    record.P = ray.PointAtParameter(record.T);
                    record.Normal = (record.P - m_Center) / Radius;
                    record.Material = m_Material;
                    return true;
                }
            }

            return false;
        }
    }
}
