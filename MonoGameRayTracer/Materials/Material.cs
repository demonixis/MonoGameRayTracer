using Microsoft.Xna.Framework;
using System;

namespace MonoGameRayTracer.Materials
{
    public abstract class Material
    {
        public abstract bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered);
    }
}
