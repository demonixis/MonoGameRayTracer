using System;
using Microsoft.Xna.Framework;
using MonoGameRayTracer.DataStructure;
using MonoGameRayTracer.Materials;

namespace MonoGameRayTracer.Primitives
{
    public class Cube : Hitable
    {
        private Vector3 m_Min;
        private Vector3 m_Max;

        public Cube(Vector3 min, Vector3 max, Material material)
        {
            m_Min = min;
            m_Max = max;
            m_Material = material;
            m_BoundingBox = new AABB(min, max);
        }

        public override bool BoundingBox(ref AABB box)
        {
            box = m_BoundingBox;
            return true;
        }

        public override bool Hit(ref Ray ray, float min, float max, ref HitRecord record)
        {
            var origin = ray.Origin;
            var direction = ray.Direction;

            var tmin = (m_Min.X - origin.X) / direction.X;
            var tmax = (m_Max.X - origin.X) / direction.X;

            if (tmin > tmax)
            {
                var tmp = tmin;
                tmin = tmax;
                tmax = tmp;
            }

            var tymin = (m_Min.Y - origin.Y) / direction.Y;
            var tymax = (m_Max.Y - origin.Y) / direction.Y;

            if (tymin > tymax)
            {
                var tmp = tmin;
                tmin = tmax;
                tmax = tmp;
            }

            if ((tmin > tymax) || (tymin > tmax))
                return false;

            if (tymin > tmin)
                tmin = tymin;

            if (tymax < tmax)
                tmax = tymax;

            float tzmin = (m_Min.Z - origin.Z) / direction.Z;
            float tzmax = (m_Max.Z - origin.Z) / direction.Z;

            if (tzmin > tzmax)
            {
                var tmp = tzmin;
                tzmin = tzmax;
                tzmax = tmp;
            }

            if ((tmin > tzmax) || (tzmin > tmax))
                return false;

            if (tzmin > tmin)
                tmin = tzmin;

            if (tzmax < tmax)
                tmax = tzmax;

            var center = m_Max - m_Min;

            record.T = Vector3.Distance(origin, center);
            record.P = ray.PointAtParameter(record.T);
            record.Normal = (record.P - center);
            record.Material = m_Material;
        }
    }
}
