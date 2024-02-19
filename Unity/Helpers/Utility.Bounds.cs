using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static Bounds GetMeshBounds(GameObject go)
        {
            var comps = go.GetComponentsInChildren<Renderer>();
            return comps.GetCombinedBounds();
        }

        public static Bounds GetColliderBounds(GameObject go)
        {
            var comps = go.GetComponentsInChildren<Collider>();
            return comps.GetCombinedBounds();
        }

        public static Bounds BoundsFromMinMax(Vector3 min, Vector3 max)
        {
            Vector3 center = (min + max) / 2;
            return new Bounds(center, (max - min).Abs());
        }
    }
}