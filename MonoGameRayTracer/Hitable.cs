using MonoGameRayTracer.DataStructure;
using MonoGameRayTracer.Materials;

namespace MonoGameRayTracer
{
    public abstract class Hitable
    {
        protected Material m_Material;
        protected AABB m_BoundingBox;

        public Material Material => m_Material;

        public abstract bool Hit(ref Ray ray, float min, float max, ref HitRecord record);
        public abstract bool BoundingBox(ref AABB box);
    }
}
