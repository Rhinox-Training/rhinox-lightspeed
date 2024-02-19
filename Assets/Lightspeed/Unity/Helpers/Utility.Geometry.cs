using System.Collections.Generic;
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
        
        /// <summary>
        /// Determines if the given point is inside the polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="testPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public static bool IsPointInPolygon(Vector2[] polygon, Vector2 testPoint)
        {
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y ||
                    polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y)
                {
                    if (polygon[i].x + (testPoint.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < testPoint.x)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
        
        public static bool IsClockWise(IList<Vector2> vertices)
        {
            // Calculate the signed Area of the polygon according to: https://en.wikipedia.org/wiki/Shoelace_formula
            float total = 0;
            for (int i = 0; i < vertices.Count; ++i)
            {
                var p1 = vertices[i];
                var p2 = (i + 1 >= vertices.Count) ? vertices[0] : vertices[i + 1];
                total += SumOverEdge(p1, p2);
            }

            return total > 0;
        }

        private static float SumOverEdge(Vector2 p1, Vector2 p2)
        {
            return (p2.x - p1.x) * (p2.y + p1.y);
        }

        /// <summary>
        /// Projects a point (shortest distance) onto a line described by an originPoint and axis (onNormal)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="lineAxis"></param>
        /// <param name="lineOrigin"></param>
        /// <returns></returns>
        public static Vector3 ProjectOnLine(Vector3 point, Vector3 lineAxis, Vector3 lineOrigin)
        {
            Vector3 diffVec = point - lineOrigin;
            Vector3 projectedDiffVec = Vector3.Project(diffVec, lineAxis);
            Vector3 projectedPoint = projectedDiffVec + lineOrigin;
            return projectedPoint;
        }

        /// <summary>
        /// Rotates a point onto a line, around the lineOrigin
        /// </summary>
        /// <param name="point"></param>
        /// <param name="lineAxis"></param>
        /// <param name="lineOrigin"></param>
        /// <returns></returns>
        public static Vector3 RotateOnLine(Vector3 point, Vector3 lineAxis, Vector3 lineOrigin)
        {
            Vector3 diffVec = point - lineOrigin;
            float length = diffVec.magnitude;
            Vector3 projectedPoint = lineAxis.normalized * length + lineOrigin;
            return projectedPoint;
        }

        public static Pose Lerp(Pose start, Pose end, float t)
        {
            return new Pose()
            {
                position = Vector3.Lerp(start.position, end.position, t),
                rotation = Quaternion.Lerp(start.rotation, end.rotation, t)
            };
        }

        public static Pose Slerp(Pose start, Pose end, float t)
        {
            return new Pose()
            {
                position = Vector3.Slerp(start.position, end.position, t),
                rotation = Quaternion.Slerp(start.rotation, end.rotation, t)
            };
        }
    }
}