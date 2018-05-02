using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameRayTracer
{
    public struct AABoundingBox
    {
        private Vector3 m_Minimum;
        private Vector3 m_Maximum;

        public Vector3 Min => m_Minimum;
        public Vector3 Max => m_Maximum;

        public AABoundingBox(Vector3 min, Vector3 max)
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
    }
}
