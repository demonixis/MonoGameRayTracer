using Microsoft.Xna.Framework;
using MonoGameRayTracer.DataStructure;
using MonoGameRayTracer.Materials;
using MonoGameRayTracer.Utils;
using System;

namespace MonoGameRayTracer.Primitives
{
    public class Sphere : Hitable
    {
        private Vector3 m_Center;
        private float m_Radius;
        private float m_Radius2;

        public Vector3 Center => m_Center;
        public float Radius => m_Radius;

        public Sphere(Vector3 center, float radius, Material material)
            : this(ref center, ref radius, material)
        {
        }

        public Sphere(ref Vector3 center, ref float radius, Material material)
        {
            m_Center = center;
            m_Radius = radius;
            m_Material = material;
            m_Material.Hitable = this;
            m_Radius2 = radius * radius;

            var bounds = new Vector3(m_Radius);
            m_BoundingBox = new AABB(m_Center - bounds, m_Center + bounds);
        }

        public override bool Hit(ref Ray ray, float min, float max, ref HitRecord record)
        {
            var rayDirection = ray.Direction;
            var oc = ray.Origin - m_Center;
            var a = Vector3.Dot(rayDirection, rayDirection);
            var b = Vector3.Dot(oc, rayDirection);
            var c = Vector3.Dot(oc, oc) - m_Radius2;
            var disciminent = b * b - a * c;

            if (disciminent > 0)
            {
                var temp = (-b - Mathf.Sqrt(b * b - a * c)) / a;

                if (temp < max && temp > min)
                {
                    record.T = temp;
                    record.P = ray.PointAtParameter(record.T);
                    record.Normal = (record.P - m_Center) / Radius;
                    record.Material = m_Material;

                    UpdateSphereUV(ref record);

                    return true;
                }

                temp = (-b + Mathf.Sqrt(b * b - a * c)) / a;

                if (temp < max && temp > min)
                {
                    record.T = temp;
                    record.P = ray.PointAtParameter(record.T);
                    record.Normal = (record.P - m_Center) / Radius;
                    record.Material = m_Material;

                    UpdateSphereUV(ref record);

                    return true;
                }
            }

            return false;
        }

        public override bool BoundingBox(ref AABB box)
        {
            box = m_BoundingBox;
            return true;
        }

        private void UpdateSphereUV(ref HitRecord record)
        {
            var p = (record.P - m_Center) / m_Radius;
            var phi = Mathf.Atan2(p.Z, p.X);
            var theta = Mathf.Asin(p.Y);
            record.U = 1.0f - (phi + MathHelper.Pi) / (2.0f * MathHelper.Pi);
            record.V = (theta + MathHelper.Pi / 2.0f) / MathHelper.Pi;
        }
    }
}
