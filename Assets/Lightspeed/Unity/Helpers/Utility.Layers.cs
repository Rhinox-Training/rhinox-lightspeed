using System.Linq;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static void SetLayerRecursively<T>(this T obj, int newLayer) where T : Component
        {
            obj.gameObject.SetLayerRecursively(newLayer);
        }

        public static void SetLayerRecursively(this GameObject obj, int newLayer)
        {
            if (null == obj)
                return;

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (child == null)
                    continue;

                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        
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