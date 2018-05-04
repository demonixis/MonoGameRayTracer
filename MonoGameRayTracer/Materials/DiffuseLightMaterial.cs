using System;
using Microsoft.Xna.Framework;
using MonoGameRayTracer.Textures;

namespace MonoGameRayTracer.Materials
{
    public class DiffuseLightMaterial : Material
    {
        private Texture m_EmissiveTexture;
        
        public DiffuseLightMaterial(Texture emissive)
        {
            m_EmissiveTexture = emissive;
        }

        public override bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered) => false;

        public override Vector3 Emitted(ref float u, ref float v, ref Vector3 p) => m_EmissiveTexture.Tex2D(u, v, ref p);
    }
}
