using Microsoft.Xna.Framework;
using MonoGameRayTracer.Textures;
using System;

namespace MonoGameRayTracer.Materials
{
    public abstract class Material
    {
        protected Texture m_Texture;
        protected Vector3 m_Albedo;
        protected bool m_TextureEnabled;

        public Vector3 Albedo => m_Albedo;
        public Hitable Hitable { get; set; }
        public abstract bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered);
        public virtual Vector3 Emitted(ref float u, ref float v, ref Vector3 p) => Vector3.Zero;
    }
}
