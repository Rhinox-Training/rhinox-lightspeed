using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static class RectExtensions
    {
        public static Rect AlignLeft(this Rect rect, float width)
        {
            rect.width = width;
            return rect;
        }

        public static Rect AlignCenter(this Rect rect, float width)
        {
            rect.x = (float)(rect.x + rect.width * 0.5 - width * 0.5);
            rect.width = width;
            return rect;
        }

        public static Rect AlignCenter(this Rect rect, float width, float height)
        {
            rect.x = (float)(rect.x + rect.width * 0.5 - width * 0.5);
            rect.y = (float)(rect.y + rect.height * 0.5 - height * 0.5);
            rect.width = width;
            rect.height = height;
            return rect;
        }

        public static Rect AlignRight(this Rect rect, float width)
        {
            rect.x = rect.x + rect.width - width;
            rect.width = width;
            return rect;
        }

        public static Rect AlignRight(this Rect rect, float width, bool clamp)
        {
            if (clamp)
            {
                rect.xMin = Mathf.Max(rect.xMax - width, rect.xMin);
                return rect;
            }

            rect.x = rect.x + rect.width - width;
            rect.width = width;
            return rect;
        }
        
        
        public static Rect SetX(this Rect rect, float x)
        {
            rect.x = x;
            return rect;
        }
        
        public static Rect AddX(this Rect rect, float x)
        {
            rect.x += x;
            return rect;
        }
        
        public static Rect SetY(this Rect rect, float y)
        {
            rect.y = y;
            return rect;
        }
        
        public static Rect AddY(this Rect rect, float y)
        {
            rect.y += y;
            return rect;
        }

        public static Rect VerticalPadding(this Rect rect, float padding)
        {
            rect.y += padding;
            rect.height -= (2.0f * padding);
            return rect;
        }
        
        public static Rect VerticalPadding(this Rect rect, float top, float bottom)
        {
            rect.y += top;
            rect.height -= (top + bottom);
            return rect;
        }
        
        public static Rect HorizontalPadding(this Rect rect, float padding)
        {
            rect.x += padding;
            rect.width -= (2.0f * padding);
            return rect;
        }
        
        public static Rect HorizontalPadding(this Rect rect, float left, float right)
        {
            rect.x += left;
            rect.width -= (left + right);
            return rect;
        }

        public static Rect Padding(this Rect rect, float padding)
        {
            rect.y += padding;
            rect.height -= (2.0f * padding);
            rect.x += padding;
            rect.width -= (2.0f * padding);
            return rect;
        }

        public static Rect Padding(this Rect rect, float horizontalPadding, float verticalPadding)
        {
            rect.y += verticalPadding;
            rect.height -= (2.0f * verticalPadding);
            rect.x += horizontalPadding;
            rect.width -= (2.0f * horizontalPadding);
            return rect;
        }
        
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