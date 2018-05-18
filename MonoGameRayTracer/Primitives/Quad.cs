using Microsoft.Xna.Framework;
using MonoGameRayTracer.DataStructure;
using MonoGameRayTracer.Materials;

namespace MonoGameRayTracer.Primitives
{
    public class Quad : Hitable
    {
        public enum Axis
        {
            XY = 0, XZ, YZ
        }

        public struct RectF
        {
            public float X;
            public float Y;
            public float Width;
            public float Height;
        }

        private float m_Start0;
        private float m_Start1;
        private float m_End0;
        private float m_End1;
        private float m_K;
        private bool m_InvertNormal;
        private Axis m_Axis;

        public Quad(Axis axis, bool invertNormal, RectF rect, float k, Material material)
        {
            m_Axis = axis;
            m_InvertNormal = invertNormal;
            m_K = k;
            m_Start0 = rect.X;
            m_Start1 = rect.X + rect.Width;
            m_End0 = rect.Y;
            m_End1 = rect.Y + rect.Height;
            m_Material = material;

            var min = new Vector3(m_Start0, m_End0, m_K - 0.0001f);
            var max = new Vector3(m_Start1, m_End1, m_K + 0.0001f);

            if (axis == Axis.XZ)
            {
                min = new Vector3(m_Start0, m_K - 0.0001f, m_End0);
                max = new Vector3(m_Start1, m_K + 0.0001f, m_End1);
            }
            else if (axis == Axis.YZ)
            {
                min = new Vector3(m_K - 0.0001f, m_Start0, m_End0);
                max = new Vector3(m_K + 0.0001f, m_Start1, m_End1);
            }

            m_BoundingBox = new AABB(min, max);
        }

        public Quad(Axis axis, bool invertNormal, float x0, float x1, float y0, float y1, float k, Material material)
        {
            m_Axis = axis;
            m_InvertNormal = invertNormal;
            m_K = k;
            m_Start0 = x0;
            m_Start1 = x1;
            m_End0 = y0;
            m_End1 = y1;
            m_Material = material;

            var min = new Vector3(m_Start0, m_End0, m_K - 0.0001f);
            var max = new Vector3(m_Start1, m_End1, m_K + 0.0001f);

            if (axis == Axis.XZ)
            {
                min = new Vector3(m_Start0, m_K - 0.0001f, m_End0);
                max = new Vector3(m_Start1, m_K + 0.0001f, m_End1);
            }
            else if (axis == Axis.YZ)
            {
                min = new Vector3(m_K - 0.0001f, m_Start0, m_End0);
                max = new Vector3(m_K + 0.0001f, m_Start1, m_End1);
            }

            m_BoundingBox = new AABB(min, max);
        }

        public override bool BoundingBox(ref AABB box)
        {
            box = m_BoundingBox;
            return true;
        }

        public override bool Hit(ref Ray ray, float min, float max, ref HitRecord record)
        {
            if (m_Axis == Axis.XY)
                return HitAxisXY(ref ray, ref record);
            else if (m_Axis == Axis.XZ)
                return HitAxisXZ(ref ray, ref record);

            return HitAxisYZ(ref ray, ref record);
        }

        private bool HitAxisXY(ref Ray ray, ref HitRecord record)
        {
            var rayOrigin = ray.Origin;
            var rayDirection = ray.Direction;
            var t = (m_K - rayOrigin.Z) / rayDirection.Z;

            var x = rayOrigin.X + t * rayDirection.X;
            var y = rayOrigin.Y + t * rayDirection.Y;

            if (x < m_Start0 || x > m_Start1 || y < m_End0 || y > m_End1)
                return false;

            record.U = (x - m_Start0) / (m_Start1 - m_Start0);
            record.V = (y - m_End0) / (m_End1 - m_End0);
            record.T = t;
            record.Material = m_Material;
            record.P = ray.PointAtParameter(t);
            record.Normal = new Vector3(0, 0, m_InvertNormal ? -1 : 1);
            return true;
        }

        private bool HitAxisXZ(ref Ray ray, ref HitRecord record)
        {
            var rayOrigin = ray.Origin;
            var rayDirection = ray.Direction;
            var t = (m_K - rayOrigin.Y) / rayDirection.Y;

            var x = rayOrigin.X + t * rayDirection.X;
            var z = rayOrigin.Z + t * rayDirection.Z;

            if (x < m_Start0 || x > m_Start1 || z < m_End0 || z > m_End1)
                return false;

            record.U = (x - m_Start0) / (m_Start1 - m_Start0);
            record.V = (z - m_End0) / (m_End1 - m_End0);
            record.T = t;
            record.Material = m_Material;
            record.P = ray.PointAtParameter(t);
            record.Normal = new Vector3(0, m_InvertNormal ? -1 : 1, 0);
            return true;
        }

        private bool HitAxisYZ(ref Ray ray, ref HitRecord record)
        {
            var rayOrigin = ray.Origin;
            var rayDirection = ray.Direction;
            var t = (m_K - rayOrigin.X) / rayDirection.X;

            var y = rayOrigin.Y + t * rayDirection.Y;
            var z = rayOrigin.Z + t * rayDirection.Z;

            if (y < m_Start0 || y > m_Start1 || z < m_End0 || z > m_End1)
                return false;

            record.U = (y - m_Start0) / (m_Start1 - m_Start0);
            record.V = (z - m_End0) / (m_End1 - m_End0);
            record.T = t;
            record.Material = m_Material;
            record.P = ray.PointAtParameter(t);
            record.Normal = new Vector3(m_InvertNormal ? -1 : 1, 0, 0);
            return true;
        }
    }
}
