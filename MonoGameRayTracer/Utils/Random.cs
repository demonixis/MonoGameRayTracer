using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Threading;

namespace MonoGameRayTracer
{
    public static class Random
    {
        private static Dictionary<int, System.Random> m_RandomDico = new Dictionary<int, System.Random>();

        public static System.Random SafeRandom
        {
            get
            {
                var current = Thread.CurrentThread.ManagedThreadId;

                if (!m_RandomDico.ContainsKey(current))
                    m_RandomDico.Add(current, new System.Random());

                return m_RandomDico[current];
            }
        }

        public static float Value => (float)SafeRandom.NextDouble();
        public static Vector3 Float3Sqrt => new Vector3(Value, Value, Value);
        public static Vector3 Float3 => new Vector3(Value * Value, Value * Value, Value * Value);
        public static Color Color => new Color(Value, Value, Value);
    }
}
