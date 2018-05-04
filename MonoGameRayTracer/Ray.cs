using Microsoft.Xna.Framework;
using MonoGameRayTracer.Materials;

namespace MonoGameRayTracer
{
    public struct HitRecord
    {
        public float U;
        public float V;
        public float T;
        public Vector3 P;
        public Vector3 Normal;
        public Material Material;
    }

    public struct Ray
    {
        private Vector3 m_Origin;
        private Vector3 m_Direction;

        public Vector3 Origin => m_Origin;
        public Vector3 Direction => m_Direction;
        public Vector3 PointAtParameter(float t) => m_Origin + t * m_Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            m_Origin = origin;
            m_Direction = direction;
        }

        public Ray(ref Vector3 origin, ref Vector3 direction)
        {
            m_Origin = origin;
            m_Direction = direction;
        }
    }
}
