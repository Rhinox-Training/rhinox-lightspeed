using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static class SeparatingAxesTheorem
    {
        public class Penetration
        {
            public Vector3 Axis;
            public Overlap Overlap;
            public float Distance => Overlap.Amount;
            public bool HasOverlap => Overlap.HasOverlap;
        }

        public class Overlap
        {
            public float Begin;
            public float End;
            public bool Inverted;

            public float Amount => End - Begin;

            public bool HasOverlap => Amount > 0.0f;
        }

        private static List<Vector3> GetAllAxes(IConvex a, IConvex b)
        {
            List<Vector3> axes = new List<Vector3>();

            var aAxes = a.GetAxes();
            var bAxes = b.GetAxes();

            foreach (var aAxis in aAxes)
                axes.Add(aAxis);

            foreach (var bAxis in bAxes)
            {
                axes.Add(bAxis);
                foreach (var aAxis in aAxes)
                    axes.Add(Vector3.Cross(aAxis, bAxis));
            }

            return axes;
        }
        
        public static IEnumerable<Penetration> GetPenetrations(this IConvex a, IConvex b, params Vector3[] axes)
        {
            var aVerts = a.GetVertices();
            var bVerts = b.GetVertices();

            var penetrations = new List<Penetration>();

            foreach (Vector3 axis in axes)
            {
                if (axis.LossyEquals(Vector3.zero)) 
                    continue;
                
                var overlap = FindOverlap(axis, bVerts, aVerts);

                if (overlap != null)
                {
                    // var dir = 
                    // Debug.DrawLine();
                    penetrations.Add(new Penetration
                    {
                        Axis = axis,
                        Overlap = overlap
                    });
                }
            }

            return penetrations;
        }
        
        public static bool IsColliding(this IConvex a, IConvex b, out ICollection<Penetration> penetrations)
        {
            List<Vector3> axes = GetAllAxes(a, b);

            var aVerts = a.GetVertices();
            var bVerts = b.GetVertices();

            penetrations = new List<Penetration>();

            foreach (Vector3 axis in axes)
            {
                if (axis.LossyEquals(Vector3.zero)) 
                    continue;

                var overlap  = FindOverlap(axis, bVerts, aVerts);
                
                // Separating Axis Found: Early Out
                if (overlap.Amount < 0)
                    return false;
                
                if (overlap.Amount > 0)
                {
                    penetrations.Add(new Penetration
                    {
                        Axis = axis,
                        Overlap = overlap
                    });
                }
            }

            return true;
        }

        private static Overlap FindOverlap(Vector3 axis, IList<Vector3> bVerts, IList<Vector3> aVerts)
        {
            float aProjMin = float.MaxValue, bProjMin = float.MaxValue;
            float aProjMax = float.MinValue, bProjMax = float.MinValue;
            int aMinIndex = -1, aMaxIndex = -1, bMinIndex = -1, bMaxIndex = -1;
            
            for (int i = 0; i < bVerts.Count; i++)
            {
                float val = Vector3.Dot(bVerts[i], axis);

                if (val < bProjMin)
                {
                    bProjMin = val;
                    bMinIndex = i;
                }

                if (val > bProjMax)
                {
                    bProjMax = val;
                    bMaxIndex = i;
                }
            }

            for (int i = 0; i < aVerts.Count; i++)
            {
                float val = Vector3.Dot(aVerts[i], axis);

                if (val < aProjMin)
                {
                    aProjMin = val;
                    aMinIndex = i;
                }

                if (val > aProjMax)
                {
                    aProjMax = val;
                    aMaxIndex = i;
                }
            }

            return FindOverlap(aProjMin, aProjMax, bProjMin, bProjMax);
        }

        private static Overlap FindOverlap(float aMin, float aMax, float bMin, float bMax)
        {
            // Possible Cases
            // 1.  a- a+ b- b+ 
            // 2.  a- b- a+ b+
            // 3.  a- b- b+ a+
            // 4.  b- a- b+ a+
            // 5.  b- b+ a- a+
            // 6.  b- a- a+ b+
            
            if (aMin < bMin)
            {
                // 3
                if (aMax > bMax)
                    return new Overlap
                    {
                        Begin = bMin,
                        End = bMax,
                        Inverted = true
                    };

                // 1, 2
                return new Overlap
                {
                    Begin = bMin,
                    End = aMax,
                    Inverted = true
                };
            }
            
            // 6
            if (aMax < bMax)
                return new Overlap
                {
                    Begin = aMin,
                    End = aMax,
                    Inverted = false
                };

            // 4, 5
            return new Overlap
            {
                Begin = aMin,
                End = bMax,
                Inverted = false
            };
        }
    }
}