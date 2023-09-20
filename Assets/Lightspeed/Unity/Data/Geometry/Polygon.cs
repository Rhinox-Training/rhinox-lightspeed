using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public struct Polygon : IConvex
    {
        public Vector3[] Corners;
        public Vector3 Center { get; private set; }
        
        private Vector3[] _axes;
        
        public static Polygon FromClockwiseCorners(params Vector2[] corners)
        {
            var corners3D = corners.Select(x => new Vector3(x.x, 0, x.y)).ToArray();
            return new Polygon(corners3D);
        }
        
        public static Polygon FromClockwiseCorners(params Vector3[] corners)
        {
            return new Polygon(corners);
        }
        
        private Polygon(Vector3[] corners)
        {
            Corners = corners;
            Center = Corners.GetAverage();
            _axes = null;
        }
        
        public IList<Vector3> GetAxes()
        {
            if (_axes == null)
            {
                _axes = new Vector3[Corners.Length];

                for (var i = 0; i < Corners.Length; i++)
                {
                    int nextI = (i + 1) % Corners.Length;
                    _axes[i] = Corners[nextI] - Corners[i];
                }
            }

            return _axes;
        }

        public IList<Vector3> GetVertices()
        {
            return Corners;
        }
    }
}