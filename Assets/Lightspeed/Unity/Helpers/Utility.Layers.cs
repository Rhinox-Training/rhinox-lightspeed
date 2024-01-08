using System.Linq;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static string[] GetLayerNames()
        {
            //there are only 32 layer fields in Unity
            return Enumerable.Range(0, 32)
                             .Select(index => LayerMask.LayerToName(index))
                             .Where(l => !string.IsNullOrEmpty(l))
                             .ToArray();
        }
    }
}