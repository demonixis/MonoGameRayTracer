using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MonoGameRayTracer
{
    public static class Random
    {
        private static Dictionary<Thread, System.Random> m_RandomDico = new Dictionary<Thread, System.Random>();
        private static System.Random m_random = new Pcg.PcgRandom(DateTime.Now.Millisecond);

        public static float Value
        {
            get
            {
                //return (float)m_random.NextDouble();

                var current = Thread.CurrentThread;

                if (!m_RandomDico.ContainsKey(current))
                    m_RandomDico.Add(current, new System.Random(DateTime.Now.Millisecond));

                return (float)m_RandomDico[current].NextDouble();
                
            }
        }
        public static Vector3 Vector3 => new Vector3(Value, Value, Value);
        public static Vector3 Vector3Twice => new Vector3(Value * Value, Value * Value, Value * Value);
        public static Color Color => new Color(Value, Value, Value);
    }
}
