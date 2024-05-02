using UnityEngine;
using UnityEngine.UIElements;

namespace Rhinox.Lightspeed
{
    public static class UIElementsExtensions
    {
        public static void SetPadding(this IStyle style, float padding)
        {
            style.paddingLeft = padding;
            style.paddingRight = padding;
            style.paddingTop = padding;
            style.paddingBottom = padding;
        }
        public static void SetPadding(this IStyle style, Vector4 padding)
        {
            style.paddingLeft = padding.x;
            style.paddingRight = padding.y;
            style.paddingTop = padding.z;
            style.paddingBottom = padding.w;
        }
    }
}