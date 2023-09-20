
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public struct Square : IConvex
    {
        public Vector3[] Axes { get; }
        public Vector3[] Vertices { get; }

        public Square(params Vector2[] vertices)
        {
            Vertices = vertices.Select(x => new Vector3(x.x, x.y, 0.0f)).ToArray();
            Axes = new[]
            {
                Vector3.right,
                Vector3.up
            };
        }

        public IList<Vector3> GetAxes() => Axes;

        public IList<Vector3> GetVertices() => Vertices;
    }
}