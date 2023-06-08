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
        }

        public class Overlap
        {
            public float Begin;
            public float End;
            public bool Inverted;
            
            public Vector3 A;
            public Vector3 B;

            public float Amount => End - Begin;
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

            float minOverlap = 0;
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
                    // var dir = 
                    // Debug.DrawLine();
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

            var overlap = FindOverlap(aProjMin, aProjMax, bProjMin, bProjMax);
            if (overlap != null)
            {
                if (overlap.Inverted)
                {
                    overlap.A = bVerts[bMinIndex];
                    overlap.B = aVerts[aMaxIndex];
                }
                else
                {
                    overlap.A = aVerts[aMinIndex];
                    overlap.B = bVerts[bMaxIndex];
                }
            }

            return overlap;
        }

        private static Overlap FindOverlap(float aMin, float aMax, float bMin, float bMax)
        {
            if (aMin < bMin)
            {
                if (aMax < bMin)
                    return new Overlap
                    {
                        Begin = bMin,
                        End = aMax,
                        Inverted = true
                    };

                return new Overlap
                {
                    Begin = bMin,
                    End = aMax,
                    Inverted = true
                };
            }

            if (bMax < aMin)
                return new Overlap
                {
                    Begin = aMin,
                    End = bMax,
                    Inverted = false
                };

            return new Overlap
            {
                Begin = aMin,
                End = bMax,
                Inverted = false
            };
        }
    }
}