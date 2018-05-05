using MonoGameRayTracer.Utils;
using System.Numerics;

namespace MonoGameRayTracer.Textures
{
    public class NoiseTexture : Texture
    {
        private PerlinNoise m_Noise;

        public NoiseTexture()
        {
            m_Noise = new PerlinNoise();
        }

        public override Vector3 Tex2D(ref float u, ref float v, ref Vector3 vector) => Vector3.One * m_Noise.Noise(ref vector);
        public override void Tex2D(ref HitRecord record, ref Vector3 result) => result = Vector3.One * m_Noise.Noise(ref record.P);
    }
}
