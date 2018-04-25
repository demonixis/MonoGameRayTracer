using Microsoft.Xna.Framework;

namespace MonoGameRayTracer
{
    public class Camera
    {
        private Vector3 m_Origin;
        private Vector3 m_LowerLeftCorner;
        private Vector3 m_Horizontal;
        private Vector3 m_Vertical;

        public Camera()
        {
            m_LowerLeftCorner = new Vector3(-2.0f, -1.0f, -1.0f);
            m_Horizontal = new Vector3(4.0f, 0.0f, 0.0f);
            m_Vertical = new Vector3(0.0f, 2.0f, 0.0f);
            m_Origin = new Vector3(0.0f, 0.0f, 0.0f);
        }

        public Ray GetRay(float u, float v) => new Ray(m_Origin, m_LowerLeftCorner + u * m_Horizontal + v * m_Vertical - m_Origin);

        public void Move(float x, float y, float z)
        {
            m_Origin.X += x;
            m_Origin.Y += y;
            m_Origin.Z += z;
        }
    }
}
