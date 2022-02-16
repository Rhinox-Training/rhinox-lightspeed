using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static float SqrDistanceBetweenSegments(Vector3 seg1Start, Vector3 seg1End, Vector3 seg2Start, Vector3 seg2End)
        {
            // TODO check intersection

            var d1 = (seg1End - seg1Start).normalized;
            var d2 = (seg2End - seg2Start).normalized;

            var normal = Vector3.Cross(d1, d2);
            if (normal.sqrMagnitude <= 0.0f) // Parallel
            {
                // https://math.stackexchange.com/questions/1347604/find-3d-distance-between-two-parallel-lines-in-simple-way
                float distance = Vector3.Dot((seg1End - seg2End), d2);
                Vector3 newRefPoint = distance * d2 + seg2End;
                return newRefPoint.SqrDistanceTo(seg1End);
            }

            var n1 = Vector3.Cross(d1, -normal);
            var n2 = Vector3.Cross(d2, normal);

            var c1 = seg1Start + ((Vector3.Dot((seg2Start - seg1Start), n2) / Vector3.Dot(d1, n2)) * d1);
            var c2 = seg2Start + ((Vector3.Dot((seg1Start - seg2Start), n1) / Vector3.Dot(d2, n1)) * d2);

            return ClampToSegment(seg1Start, seg1End, c1).SqrDistanceTo(ClampToSegment(seg2Start, seg2End, c2));
        }

        public static Vector3 GetApproximateIntersectionSegment(Vector3 seg1Start, Vector3 seg1End, Vector3 seg2Start, Vector3 seg2End)
        {
            var d1 = (seg1End - seg1Start).normalized;
            var d2 = (seg2End - seg2Start).normalized;

            var normal = Vector3.Cross(d1, d2);

            var n1 = Vector3.Cross(d1, -normal);
            var n2 = Vector3.Cross(d2, normal);

            var c1 = seg1Start + ((Vector3.Dot((seg2Start - seg1Start), n2) / Vector3.Dot(d1, n2)) * d1);
            var c2 = seg2Start + ((Vector3.Dot((seg1Start - seg2Start), n1) / Vector3.Dot(d2, n1)) * d2);

            return (ClampToSegment(seg1Start, seg1End, c1) + (ClampToSegment(seg2Start, seg2End, c2))) / 2.0f;
        }

        public static Vector3 GetApproximateIntersection(Vector3 aStart, Vector3 aDir, Vector3 bStart, Vector3 bDir)
        {
            var d1 = aDir.normalized;
            var d2 = bDir.normalized;

            var normal = Vector3.Cross(d1, d2);

            var n1 = Vector3.Cross(d1, -normal);
            var n2 = Vector3.Cross(d2, normal);

            var c1 = aStart + ((Vector3.Dot((bStart - aStart), n2) / Vector3.Dot(d1, n2)) * d1);
            var c2 = bStart + ((Vector3.Dot((aStart - bStart), n1) / Vector3.Dot(d2, n1)) * d2);

            return (c1 + c2) / 2.0f;
        }

        private static Vector3 ClampToSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            var toStart = (point - start).sqrMagnitude;
            var toEnd = (point - end).sqrMagnitude;
            var segment = (start - end).sqrMagnitude;
            if (toStart > segment || toEnd > segment) return toStart > toEnd ? end : start;
            return point;
        }
    }
}