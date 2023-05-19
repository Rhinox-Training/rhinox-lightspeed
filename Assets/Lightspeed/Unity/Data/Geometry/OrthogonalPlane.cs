using System;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public struct OrthogonalPlane
    {
        public Axis Axis;
        public Vector3 Point;

        public OrthogonalPlane(Axis axis, Vector3 point)
        {
            if (axis.IsSingleFlag()) throw new ArgumentOutOfRangeException(nameof(axis));
            Axis = axis;
            Point = point;
        }

        public Plane ToPlane()
        {
            switch (Axis)
            {
                case Axis.X:
                    return new Plane(Vector3.right, Point);
                case Axis.Y:
                    return new Plane(Vector3.up, Point);
                case Axis.Z:
                    return new Plane(Vector3.forward, Point);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}