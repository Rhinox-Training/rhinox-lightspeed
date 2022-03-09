using System.Diagnostics;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    [DebuggerDisplay("{V1} -> {V2} -> {V3} -> {V4}")]
    public class Quad
    {
        public Vector3 V1;
        public Vector3 V2;
        public Vector3 V3;
        public Vector3 V4;

        public Quad()
        {
        }

        public Quad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            V4 = v4;
        }
    }
}