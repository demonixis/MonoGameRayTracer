using Microsoft.Xna.Framework;
using System;

namespace MonoGameRayTracer
{
    public abstract class Material
    {
        public abstract bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered);

        public static Vector3 RandomInUnitySphere()
        {
            var vector = Vector3.Zero;

            do
            {
                vector = 2.0f * Random.Vector3 - Vector3.One;
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
