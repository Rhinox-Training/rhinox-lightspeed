using Rhinox.GUIUtils;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    // TODO: migrate GUIUtils.RectExtensions to Lightspeed
    public static class RectExtensions
    {
        public static Rect PadLeft(this Rect r, float padding)
        {
            return r.VerticalPadding(padding, 0);
        }

        public static Rect PadRight(this Rect r, float padding)
        {
            return r.VerticalPadding(0, padding);
        }

        public static Vector2 GetTopLeft(this Rect r) => new Vector2(r.xMin, r.yMax);
        public static Vector2 GetTopRight(this Rect r) => new Vector2(r.xMax, r.yMax);
        public static Vector2 GetBottomLeft(this Rect r) => new Vector2(r.xMin, r.yMin);
        public static Vector2 GetBottomRight(this Rect r) => new Vector2(r.xMax, r.yMin);
    }
}