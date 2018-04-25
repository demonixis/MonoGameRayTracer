using Microsoft.Xna.Framework;
using System;

namespace MonoGameRayTracer
{
    public abstract class Material
    {
        protected static Random m_Random = new Random(DateTime.Now.Millisecond);

        public abstract bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered);

        public static Vector3 RandomInUnitySphere()
        {
            var vector = Vector3.Zero;

            do
            {
                vector = 2.0f * new Vector3((float)m_Random.NextDouble(), (float)m_Random.NextDouble(), (float)m_Random.NextDouble()) - Vector3.One;
            }
            while (vector.LengthSquared() > 1.0f);

            return vector;
        }

        public static Vector3 UnitVector(Vector3 vector) => vector / vector.Length();

        public static void MakeUnitVector(Vector3 vector)
        {
            float k = 1.0f / (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            vector *= k;
        }
    }
}
