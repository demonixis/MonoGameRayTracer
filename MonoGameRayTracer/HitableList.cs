using MonoGameRayTracer.DataStructure;
using System.Collections.Generic;

namespace MonoGameRayTracer
{
    public class HitableList : Hitable
    {
        private List<Hitable> m_List;

        public HitableList(Hitable[] list)
        {
            m_List = new List<Hitable>(list);
        }

        public HitableList(List<Hitable> list)
        {
            m_List = new List<Hitable>(list);
        }

        public override bool Hit(ref Ray ray, float min, float max, ref HitRecord record)
        {
            HitRecord tempRecord = new HitRecord();
            var hitAnything = false;
            var closestSoFar = max;

            for (var i = 0; i < m_List.Count; i++)
            {
                if (m_List[i].Hit(ref ray, min, closestSoFar, ref tempRecord))
                {
                    hitAnything = true;
                    closestSoFar = tempRecord.T;
                    record = tempRecord;
                }
            }

            return hitAnything;
        }

        public override bool BoundingBox(ref AABB box)
        {
            if (m_List.Count < 1)
                return false;

            AABB tmp = new AABB();
            var first = m_List[0].BoundingBox(ref tmp);

            if (!first)
                return false;

            box = tmp;

            for (var i = 0; i < m_List.Count; i++)
            {
                if (m_List[0].BoundingBox(ref tmp))
                    box = AABB.SurroundingBox(ref box, ref tmp);
            }

            return true;
        }
    }
}
