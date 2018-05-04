using Microsoft.Xna.Framework;
using System;

namespace MonoGameRayTracer.Materials
{
    public abstract class Material
    {
        public Hitable Hitable { get; set; }

        public abstract Vector4 Value { get; }
        public abstract bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered);
    }
}
