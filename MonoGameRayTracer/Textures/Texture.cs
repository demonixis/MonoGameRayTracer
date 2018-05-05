using System.Numerics;

namespace MonoGameRayTracer.Textures
{
    public abstract class Texture
    {
        public abstract Vector3 Tex2D(ref float u, ref float v, ref Vector3 vector);
        public abstract void Tex2D(ref HitRecord record, ref Vector3 result);
    }
}
