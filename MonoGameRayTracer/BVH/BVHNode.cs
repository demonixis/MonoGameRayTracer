using System.Collections.Generic;

namespace MonoGameRayTracer.DataStructure
{
    public class BVHNode : Hitable
    {
        private Hitable m_Left;
        private Hitable m_Right;
        private AABB m_BoundingBox;

        public BVHNode(List<Hitable> scene, int n, float t0, float t1)
        {
            var axis = (int)(Random.Value * 3.0f);

            if (axis == 0)
                scene.Sort(BoxXCompare);
            else if (axis == 1)
                scene.Sort(BoxYCompare);
            else
                scene.Sort(BoxZCompare);

            if (n == 1)
                m_Left = m_Right = scene[0];
            else if (n == 2)
            {
                m_Left = scene[0];
                m_Right = scene[1];
            }
            else
            {
                m_Left = new BVHNode(scene, n / 2, t0, t1);

                var list = new List<Hitable>();
                for (var i = n / 2; i < scene.Count; i++)
                    list.Add(scene[i]);

                m_Right = new BVHNode(list, n - n / 2, t0, t1);
            }

            var left = new AABB();
            var right = new AABB();

            if (!m_Left.BoundingBox(0, 0, ref left) || !m_Right.BoundingBox(0, 0, ref right))
                throw new System.Exception("No bounding box in bvh_node constructor");

            m_BoundingBox = AABB.SurroundingBox(ref left, ref right);
        }

        public override bool BoundingBox(float t0, float t1, ref AABB box)
        {
            box = m_BoundingBox;
            return true;
        }

        public override bool Hit(ref Ray ray, float min, float max, ref HitRecord record)
        {
            if (m_BoundingBox.Hit(ref ray, min, max))
            {
                var leftRecord = new HitRecord();
                var rightRecord = new HitRecord();
                var hitLeft = m_Left.Hit(ref ray, min, max, ref leftRecord);
                var hitRight = m_Right.Hit(ref ray, min, max, ref rightRecord);

                if (hitLeft && hitRight)
                {
                    if (leftRecord.T < rightRecord.T)
                        record = leftRecord;
                    else
                        record = rightRecord;

                    return true;

                }
                else if (hitLeft)
                {
                    record = leftRecord;
                    return true;
                }
                else if (hitRight)
                {
                    record = rightRecord;
                    return true;
                }
            }

            return false;
        }

        private int BoxXCompare(Hitable a, Hitable b)
        {
            var left = new AABB();
            var right = new AABB();

            if (!a.BoundingBox(0, 0, ref left) || !b.BoundingBox(0, 0, ref right))
                throw new System.Exception("No bounding box in bvh_node constructor");

            if (left.Min.X - right.Min.X < 0.0f)
                return -1;

            return 1;
        }

        private int BoxYCompare(Hitable a, Hitable b)
        {
            AABB left = new AABB();
            AABB right = new AABB();

            if (!a.BoundingBox(0, 0, ref left) || !b.BoundingBox(0, 0, ref right))
                throw new System.Exception("No bounding box in bvh_node constructor");

            if (left.Min.Y - right.Min.Y < 0.0f)
                return -1;

            return 1;
        }

        private int BoxZCompare(Hitable a, Hitable b)
        {
            AABB left = new AABB();
            AABB right = new AABB();

            if (!a.BoundingBox(0, 0, ref left) || !b.BoundingBox(0, 0, ref right))
                throw new System.Exception("No bounding box in bvh_node constructor");

            if (left.Min.Z - right.Min.Z < 0.0f)
                return -1;

            return 1;
        }
    }
}
