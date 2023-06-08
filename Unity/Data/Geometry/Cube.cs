using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public struct Cube : IConvex
    {
        public Vector3 Center { get; }
        public Vector3[] Axes { get; }
        public Vector3[] Vertices { get; }

        public static Cube FromExtents(Vector3 center, Vector3 extents, Vector3 forward, Vector3 up)
        {
            var right = Vector3.Cross(forward, up);
            return new Cube(center, Vector3.Scale(forward, extents), Vector3.Scale(up, extents), Vector3.Scale(right, extents));
        }
        
        public static Cube FromScaledNormals(Vector3 center, Vector3 forward, Vector3 up, Vector3 right)
        {
            return new Cube(center, forward, up, right);
        }
        
        public static Cube FromTransformBounds(Transform t, Bounds bounds)
        {
            var center = t.TransformPoint(bounds.center);
            var forward = Vector3.Scale(t.forward, bounds.extents);
            var up = Vector3.Scale(t.up, bounds.extents);
            var right = Vector3.Scale(t.right, bounds.extents);
            
            return new Cube(center, forward, up, right);
        }

        public static Cube FromRenderers(GameObject go) => FromTransformBounds(go.transform, go.GetObjectLocalBounds(colliders: Array.Empty<Collider>()));

        public static Cube FromGameObject(GameObject go) => FromTransformBounds(go.transform, go.GetObjectLocalBounds());

        public static Cube ToScreenSpace(GameObject go, Camera cam)
        {
            return FromRenderers(go).ToScreenSpace(cam);
        }

        private Cube(Vector3 center, Vector3 forward, Vector3 up, Vector3 right)
        {
            Center = center;

            Axes = new Vector3[]
            {
                forward.normalized,
                up.normalized,
                right.normalized
            };
            
            Vertices = new Vector3[]
            {
                Center + up + forward + right,
                Center + up + forward - right,
                Center + up - forward - right,
                Center + up - forward + right,
                Center - up + forward + right,
                Center - up + forward - right,
                Center - up - forward - right,
                Center - up - forward + right,
            };
        }

        private Cube(Vector3[] axes, Vector3[] verts)
        {
            Axes = axes;
            Vertices = verts;
            Center = Vertices.GetAverage();
        }

        public Cube ToScreenSpace(Camera cam)
        {
            var axes = Axes.Select(x => cam.transform.InverseTransformDirection(x)).ToArray();
            var verts = Vertices.Select(x => cam.WorldToScreenPoint(x)).ToArray();
            return new Cube(axes, verts);
        }

        public IList<Vector3> GetAxes() => Axes;
        public IList<Vector3> GetVertices() => Vertices;
    }
}