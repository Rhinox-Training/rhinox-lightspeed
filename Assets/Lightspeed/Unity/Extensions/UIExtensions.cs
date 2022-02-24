using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rhinox.Lightspeed
{
    public static class UIExtensions
    {
        public static int CountManagedObjects(this LayoutGroup layoutGroup)
        {
            int n = 0;
            foreach (var child in layoutGroup.transform)
            {
                if (child is RectTransform rt)
                    n += 1;
            }

            return n;
        }
        
        public static IEnumerable<RectTransform> GetManagedObjects(this LayoutGroup layoutGroup)
        {
            foreach (var child in layoutGroup.transform)
            {
                if (child is RectTransform rt)
                    yield return rt;
            }
        }
        
        public static Vector2 GetUiScreenPosition(this Transform transform, bool local = false)
        {
            var pos = local ? transform.localPosition : transform.position;
            var canvas = transform.GetComponentInParent<Canvas>();
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return pos;

            return Camera.main.WorldToScreenPoint(pos);
        }
        
        public static Vector2 ToVector2(this Resolution r)
        {
            return new Vector2(r.width, r.height);
        }
    }
}