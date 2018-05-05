using System;
using System.Numerics;

namespace MonoGameRayTracer.Utils
{
    public static class Mathf
    {
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

        public static Vector3 UnitVector(ref Vector3 vector) => vector / vector.Length();
        public static Vector3 UnitVector(Vector3 vector) => vector / vector.Length();

        public static void UnityVector(ref Vector3 vector, ref Vector3 result) => result = vector / vector.Length();

        public static void MakeUnitVector(ref Vector3 vector)
        {
            float k = 1.0f / (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            vector *= k;
        }

        public static bool Refract(ref Vector3 v, ref Vector3 n, float niOverNt, ref Vector3 refracted)
        {
            var uv = UnitVector(v);
            var dt = Vector3.Dot(uv, n);
            var discriminant = 1.0f - niOverNt * niOverNt * (1.0f - dt * dt);

            if (discriminant > 0)
            {
                refracted = niOverNt * (uv - n * dt) - n * (float)Math.Sqrt(discriminant);
                return true;
            }

            return false;
        }

        public static float Schlick(float cosine, float refIdx)
        {
            var r0 = (1.0f - refIdx) / (1 + refIdx);
            r0 = r0 * r0;
            return r0 + (1.0f - r0) * (float)Math.Pow((1.0f - cosine), 5.0f);
        }

        public static float Tan(float value) => (float)Math.Tan(value);
        public static float Sqrt(float value) => (float)Math.Sqrt(value);
        public static float Sin(float value) => (float)Math.Sin(value);
        public static float Cos(float value) => (float)Math.Cos(value);
        public static float Floor(float value) => (float)Math.Floor(value);
        public static float Atan2(float a, float b) => (float)Math.Atan2(a, b);
        public static float Asin(float value) => (float)Math.Asin(value);

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}
