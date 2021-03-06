﻿using Microsoft.Xna.Framework;
using MonoGameRayTracer.Utils;

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
    }
}
