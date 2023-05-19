using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static class BoundsSubdivisionUtility
    {
        public static List<Bounds> DivideAndMergeConcave(Bounds rootBounds, IEnumerable<OrthogonalPlane> cuttingPlanes, Func<Bounds, bool> discardFunc, float margin = 0.0f)
        {
            if (discardFunc == null) throw new ArgumentNullException(nameof(discardFunc));
            
            List<Bounds> boundsList = new List<Bounds>
            {
                rootBounds
            };

            foreach (var plane in cuttingPlanes)
            {
                SubdivideBounds(ref boundsList, plane.Axis, plane.Point);
            }

            // Discard pass
            boundsList.RemoveAll(x => discardFunc.Invoke(x));
            if (boundsList.Count == 0)
                return boundsList;

            // Merge pass
            CheckMergeAligning(ref boundsList);

            //go over all passing bounds and add margins
            if (margin > float.Epsilon)
            {
                for (int i = 0; i < boundsList.Count; ++i)
                {
                    boundsList[i] = boundsList[i].AddMarginToExtends(margin);
                }
            }

            return boundsList;
        }
        
        private static void SubdivideBounds(ref List<Bounds> boundsList, Axis axis, Vector3 pointOnPlane)
        {
            var subdividedBounds = new List<Bounds>();
            foreach (var bounds in boundsList)
            {
                if (bounds.TrySliceBounds(axis, pointOnPlane, out Bounds halfBounds1, out Bounds halfBounds2))
                {
                    subdividedBounds.Add(halfBounds1);
                    subdividedBounds.Add(halfBounds2);
                }
                else
                {
                    subdividedBounds.Add(bounds);
                }
            }

            boundsList = subdividedBounds;
        }

        private static void CheckMergeAligning(ref List<Bounds> boundsList)
        {
            if (boundsList == null)
                return;

            int count = boundsList.Count;
            int newCount = -1;
            while (count != newCount)
            {
                count = boundsList.Count;
                boundsList.SortBy(x => x.GetSmallestDimension());
                SinglePassSearchForAligningBounds(ref boundsList);
                newCount = boundsList.Count;
            }
        }
        
        private static void SinglePassSearchForAligningBounds(ref List<Bounds> boundsList)
        {
            for (var i = 0; i < boundsList.Count; ++i)
            {
                var bound = boundsList[i];
                for (var j = 0; j < boundsList.Count; ++j)
                {
                    if (i == j)
                        continue;
                    var otherBound = boundsList[j];
                    if (bound.IsAxisAlignedWith(otherBound))
                    {
                        bound.Encapsulate(otherBound);
                        boundsList[i] = bound;
                        boundsList.RemoveAt(j);
                        return;
                    }
                }
            }
        }
    }
}