using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static void SetTagRecursively<T>(this T obj, string newTag) where T : Component
        {
            obj.gameObject.SetTagRecursively(newTag);
        }

        public static void SetTagRecursively(this GameObject obj, string newTag)
        {
            if (null == obj)
                return;
            
            obj.tag = newTag;

            foreach (Transform child in obj.transform)
            {
                if (child == null)
                    continue;

                SetTagRecursively(child.gameObject, newTag);
            }
        }
    }
}