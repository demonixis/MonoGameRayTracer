using Microsoft.Xna.Framework;

namespace MonoGameRayTracer
{
    public static class Random
    {
        private static uint state = (uint)System.DateTime.Now.Millisecond;

        public static uint XorShift32()
        {
            var x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x;
            return x;
        }

        public static float Value => (float)XorShift32() / (float)uint.MaxValue;

        public static Vector3 Float3Sqrt => new Vector3(Value, Value, Value);

        public static Vector3 Float3()
        {
            var x = Value;
            var y = Value;
            var z = Value;
            return new Vector3(x * x, y * y, z * z);
        }

        public static Color Color => new Color(Value, Value, Value);
    }
}
