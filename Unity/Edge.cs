using System.Collections.Generic;
using Rhinox.Utilities;
using UnityEngine;

namespace Rhinox.Utilities
{    
    public class Edge
    {
        public Vector3 V1 { get; private set; }
        public Vector3 V2 { get; private set; }

        public Edge(Vector3 v1, Vector3 v2)
        {
            V1 = v1;
            V2 = v2;
            SqrLength = (V2 - V1).sqrMagnitude;
        }

        public float SqrLength { get; private set; }

        public void Flip()
        {
            var v1 = V1;
            V1 = V2;
            V2 = v1;
        }

        public Edge Flipped()
        {
            return new Edge(V2, V1);
        }
        
        public override string ToString()
        {
            return $"{V1} - {V2}";
        }
    }

    public class EdgeComparer : IEqualityComparer<Edge>
    {
        public const float Epsilon = 0.001f;

        public bool Equals(Edge a, Edge b)
        {
            // NOTE: will return true for edges that are the inverse of eachother
            if (a == null || b == null)
                return false;

            return (a.V1.LossyEquals(b.V1, Epsilon) && a.V2.LossyEquals(b.V2, Epsilon)) ||
                   (a.V1.LossyEquals(b.V2, Epsilon) && a.V2.LossyEquals(b.V1, Epsilon));
        }


        public static bool EdgeConnectsToPoint(Vector3 edgePoint, Edge e2)
        {
            return e2.V1.LossyEquals(edgePoint, Epsilon) ||
                   e2.V2.LossyEquals(edgePoint, Epsilon);
            // return EdgesAlign(e1, e2) || EdgesAlign(e1, e2.Flipped());
        }
        
        public static int GetEdgePointConnectedToEdge(Edge e1, Edge e2)
        {
            if (EdgeConnectsToPoint(e1.V1, e2))
                return 0;
            if (EdgeConnectsToPoint(e1.V2, e2))
                return 1;

            return -1;
            // return EdgesAlign(e1, e2) || EdgesAlign(e1, e2.Flipped());
        }

        public static bool EdgesAlign(Edge e1, Edge e2)
        {
            return e1.V2.LossyEquals(e2.V1, Epsilon);
        }

        public int GetHashCode(Edge edge)
        {
            // NOTE: will return the same hashcode for edges that are inverted
            // This is expected when equals would return true, the hashcode would return the same 
            var def = edge.V1.GetHashCode() ^ edge.V2.GetHashCode();
            var inv = edge.V2.GetHashCode() ^ edge.V1.GetHashCode();
            return def > inv ? def ^ inv : inv ^ def;
        }
    }
}