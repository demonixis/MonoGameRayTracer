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
    }
}
