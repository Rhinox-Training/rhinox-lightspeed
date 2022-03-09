using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    [Serializable]
    public class Spline
    {
        public List<Vector3> AnchorPoints = new List<Vector3>();
        public List<Vector3> ControlPoints = new List<Vector3>();
        public List<Vector3> Points = new List<Vector3>();
        public int PointsPerAnchor = 4;
        public float Smoothness = 0.4f;

        Vector3 CalculateSplinePoint(Vector3 start, Vector3 startControl, Vector3 end, Vector3 endControl, float t)
        {
            var omt = 1 - t;
            //Bezier Formula
            return Mathf.Pow(omt, 3) * start
                   + 3 * Mathf.Pow(omt, 2) * t * startControl
                   + 3 * omt * Mathf.Pow(t, 2) * endControl
                   + Mathf.Pow(t, 3) * end;
        }

        public void GeneratePoints()
        {
            Points.Clear();
            for (int i = 1; i < AnchorPoints.Count; i++)
            {
                for (float j = 0; j < 1.0f; j += (1.0f / PointsPerAnchor))
                {
                    var start = AnchorPoints[i - 1];
                    var end = AnchorPoints[i];
                    var previous = i > 1 ? AnchorPoints[i - 2] : start;
                    var next = i < AnchorPoints.Count - 1 ? AnchorPoints[i + 1] : end;
                    var startControl = start + (end - previous) * Smoothness;
                    if (ControlPoints.Count > i - 1)
                        startControl = ControlPoints[i - 1];
                    var endControl = end + (start - next) * Smoothness;
                    Points.Add(CalculateSplinePoint(start, startControl, end, endControl, j));
                }
            }
        }
    }
}
