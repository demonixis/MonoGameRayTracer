namespace MonoGameRayTracer
{
    public class BVHNode : Hitable
    {
        private Hitable m_Left;
        private Hitable m_Right;
        private AABoundingBox m_BoundingBox;

        public BVHNode(Hitable hitable, int n, float t0, float t1)
        {
            var axis = (int)(Random.Value * 3.0f);
            
        }

        public override bool BoundingBox(float t0, float t1, ref AABoundingBox box)
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
    }
}
