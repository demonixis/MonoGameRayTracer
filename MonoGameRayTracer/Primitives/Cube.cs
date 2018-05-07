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
        private Vector3 m_Center;
        private float m_Radius;

        public Cube(Vector3 min, Vector3 max, Material material)
        {
            m_Min = min;
            m_Max = max;
            m_Material = material;
            m_BoundingBox = new AABB(min, max);

            var sphere = BoundingSphere.CreateFromBoundingBox(new BoundingBox(min, max));

            m_Center = sphere.Center;
            m_Radius = sphere.Radius;
        }

        public override bool BoundingBox(ref AABB box)
        {
            box = m_BoundingBox;
            return true;
        }

        public override bool Hit(ref Ray ray, float min, float max, ref HitRecord record)
        {
            var rayDirection = ray.Direction;
            var rayOrigin = ray.Origin;

            var direction = AABB.VectorToArray(ref rayDirection);
            var origin = AABB.VectorToArray(ref rayOrigin);
            var vMin = AABB.VectorToArray(ref m_Min);
            var vMax = AABB.VectorToArray(ref m_Max);

            for (var a = 0; a < 3; a++)
            {
                var invD = 1.0f / direction[a];
                var t0 = (vMin[a] - origin[a]) * invD;
                var t1 = (vMax[a] - origin[a]) * invD;

                if (invD < 0.0f)
                {
                    var tmp = t0;
                    t0 = t1;
                    t1 = tmp;
                }

                min = t0 > min ? t0 : min;
                max = t1 < max ? t1 : max;

                if (max <= min)
                    return false;
            }

            record.T = Vector3.Distance(rayOrigin, m_Center);
            record.P = ray.PointAtParameter(record.T);
            record.Normal = (record.P - m_Center);
            record.Material = m_Material;

            return true;
        }
    }
}
