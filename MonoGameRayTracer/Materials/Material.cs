using System.Numerics;

namespace MonoGameRayTracer.Materials
{
    public abstract class Material
    {
        public Hitable Hitable { get; set; }
        public abstract bool Scatter(ref Ray ray, ref HitRecord record, ref Vector3 attenuation, ref Ray scattered);
        public virtual Vector3 Emitted(ref float u, ref float v, ref Vector3 p) => Vector3.Zero;
    }
}
