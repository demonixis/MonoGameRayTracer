namespace MonoGameRayTracer
{
    public abstract class Hitable
    {
        protected Material m_Material;

        public Material Material => m_Material;

        public abstract bool Hit(ref Ray ray, float min, float max, ref HitRecord record);
    }
}
