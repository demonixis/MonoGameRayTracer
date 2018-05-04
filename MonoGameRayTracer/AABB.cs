using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameRayTracer
{
    public struct AABB
    {
        private Vector3 m_Minimum;
        private Vector3 m_Maximum;

        public Vector3 Min => m_Minimum;
        public Vector3 Max => m_Maximum;

        public AABB(Vector3 min, Vector3 max)
        {
            m_Minimum = min;
            m_Maximum = max;
        }

        public bool Hit(ref Ray ray, float min, float max)
        {
            var direction = VectorToArray(ray.Direction);
            var origin = VectorToArray(ray.Origin);
            var vMin = VectorToArray(m_Minimum);
            var vMax = VectorToArray(m_Maximum);

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

            return true;
        }

        private float[] VectorToArray(Vector3 vector) => new[] { vector.X, vector.Y, vector.Z };

        public static AABB SurroundingBox(ref AABB box0, ref AABB box1)
        {
            Vector3 small = new Vector3(
                Math.Min(box0.Min.X, box1.Min.X),
                Math.Min(box0.Min.Y, box1.Min.Y),
                Math.Min(box0.Min.Z, box1.Min.Z)
            );

            Vector3 big = new Vector3(
                Math.Min(box0.Max.X, box1.Max.X),
                Math.Min(box0.Max.Y, box1.Max.Y),
                Math.Min(box0.Max.Z, box1.Max.Z)
            );

            return new AABB(small, big);
        }
    }
}
