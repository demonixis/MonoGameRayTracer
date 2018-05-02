using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameRayTracer.Utils
{
    public class PerlinNoise
    {
        private static float[] RanFloat;
        private static int[] PermX;
        private static int[] PermY;
        private static int[] PermZ;

        public PerlinNoise()
        {
            if (RanFloat == null)
            {
                RanFloat = PerlinGenerate();
                PermX = PerlinGeneratePerm();
                PermY = PerlinGeneratePerm();
                PermZ = PerlinGeneratePerm();
            }
        }

        public float Noise(ref Vector3 p)
        {
            var i = (int)(4 * p.X) & 255;
            var j = (int)(4 * p.Y) & 255;
            var k = (int)(4 * p.Z) & 255;
            return RanFloat[PermX[i] ^ PermY[j] ^ PermZ[k]];
        }

        public static void Permute(ref int[] p)
        {
            for (var i = 0; i < p.Length; i++)
            {
                var target = (int)(Random.Value * (i + 1));
                var tmp = p[i];
                p[i] = p[target];
                p[target] = tmp;
            }
        }

        public static float[] PerlinGenerate()
        {
            var p = new float[256];

            for (var i = 0; i < p.Length; i++)
                p[i] = Random.Value;

            return p;
        }

        public static int[] PerlinGeneratePerm()
        {
            var p = new int[256];
            for (var i = 0; i < p.Length; i++)
                p[i] = i;

            Permute(ref p);
            return p;
        }
    }
}
