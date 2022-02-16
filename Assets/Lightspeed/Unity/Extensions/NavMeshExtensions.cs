using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Rhinox.Lightspeed
{
    public static class NavMeshExtensions
    {
        public static float PathDistance(this NavMeshPath path, Vector3 startPos)
        {
            if (path.corners == null || !path.corners.Any()) return 0;

            var distance = Vector3.Distance(startPos, path.corners[0]);

            return distance + path.Distance();
        }

        public static float Distance(this NavMeshPath path)
        {
            if (path.corners == null || !path.corners.Any()) return 0;
            if (path.corners.Length < 2) return 0;

            float distance = 0;

            for (int i = 0; i < path.corners.Length - 1; i++)
                distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);

            return distance;
        }

        public static bool TryGetPoint(this NavMeshPath path, float d, out Vector3 point, out int nextCorner)
        {
            nextCorner = 0;
            point = Vector3.zero;
            if (path.corners == null || !path.corners.Any()) return false;

            if (path.corners.Length < 2) return false;

            float dCovered = 0;

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                var a = path.corners[i];
                var b = path.corners[i + 1];
                var dCorners = Vector3.Distance(a, b);
                if (dCovered + dCorners >= d)
                {
                    float l = (d - dCovered) / dCorners;
                    point = Vector3.Lerp(a, b, l);
                    nextCorner = i + 1;
                    return true;
                }

                dCovered += dCorners;
            }

            return false;
        }
    }
}