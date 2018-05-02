using Microsoft.Xna.Framework;

namespace MonoGameRayTracer.Textures
{
    public abstract class Texture
    {
        public abstract Vector3 Tex2D(float u, float v, ref Vector3 vector);
    }
}
