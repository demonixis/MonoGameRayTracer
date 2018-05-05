using MonoGameRayTracer.DataStructure;
using MonoGameRayTracer.Materials;

namespace MonoGameRayTracer
{
    public abstract class Hitable
    {
        protected Material m_Material;

        public Material Material => m_Material;

        public abstract bool Hit(ref Ray ray, float min, float max, ref HitRecord record);
        public abstract bool BoundingBox(float t0, float t1, ref AABB box);
    }
}
