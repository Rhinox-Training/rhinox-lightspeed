using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static class RectExtensions
    {
        // If in an event where rects aren't computed, they will all be 1 width, 1 height.
        public static bool IsValid(this Rect rect)
        {
            return rect.height > 1 || rect.width > 1;
        }
        
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
        
        public static Rect AlignCenterVertical(this Rect rect, float height)
        {
            rect.y = (float)(rect.y + rect.height * 0.5 - height * 0.5);
            rect.height = height;
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

        public static Rect AlignRight(this Rect rect, float width, bool clamp = false)
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

        public static Rect AlignRightForText(this Rect rect, string text, GUIStyle style, float padding = 4.0f)
        {
            float x = style.CalcSize(new GUIContent(text)).x;
            Rect alignedRect = rect.AlignRight(x + padding, true);
            return alignedRect;
        }

        public static Rect AlignRightBefore(this Rect rect, float width, Rect otherRect, bool clamp = false)
        {
            var r = rect.AddX(otherRect.xMin - rect.xMax);
            return r.AlignRight(width);
        }
        
        public static Rect AlignTop(this Rect rect, float height)
        {
            rect.height = height;
            return rect;
        }
        
        public static Rect AlignBottom(this Rect rect, float height)
        {
            rect.y = rect.y + rect.height - height;
            rect.height = height;
            return rect;
        }

        public static Rect MoveBeneath(this Rect rect, float height)
        {
            Rect r = new Rect(rect.x, rect.yMax, rect.width, height);
            return r;
        }
        
        public static Rect MoveDownLine(this Rect rect, int lineCount = 1, float padding = 2.0f, float lineHeight = 18f, bool autoClamp = false)
        {
            if (autoClamp)
            {
                rect.yMin += lineCount * padding + lineCount * lineHeight;
                return rect;
            }

            return rect.AddY(lineCount * padding + lineCount * lineHeight);
        }

        public static void SplitX(this Rect rect, float width, out Rect left, out Rect right)
        {
            float rightWidth = rect.width - width;
            left = rect.AlignLeft(width);
            right = rect.AlignRight(rightWidth);
        }

        public static void SplitX(this Rect rect, float width1, float width2, out Rect left, out Rect middle, out Rect right)
        {
            rect.SplitX(width1, out left, out Rect overflow);
            float offset = width2 - width1;
            overflow.SplitX(offset, out middle, out right);
        }

        public static void SplitY(this Rect rect, float height, out Rect top, out Rect bottom)
        {
            float bottomHeight = rect.height - height;
            top = rect.AlignTop(height);
            bottom = rect.AlignBottom(bottomHeight);
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

        public static Rect SetWidth(this Rect rect, float width)
        {
            rect.width = width;
            return rect;
        }
        
        public static Rect SetHeight(this Rect rect, float height)
        {
            rect.height = height;
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
            return r.HorizontalPadding(padding, 0);
        }

        public static Rect PadRight(this Rect r, float padding)
        {
            return r.HorizontalPadding(0, padding);
        }

        public static Vector2 GetTopLeft(this Rect r) => new Vector2(r.xMin, r.yMax);
        public static Vector2 GetTopRight(this Rect r) => new Vector2(r.xMax, r.yMax);
        public static Vector2 GetBottomLeft(this Rect r) => new Vector2(r.xMin, r.yMin);
        public static Vector2 GetBottomRight(this Rect r) => new Vector2(r.xMax, r.yMin);

        public static Rect Encapsulate(this Rect r, Vector2 point)
        {
            if (r.Contains(point))
                return r;

            if (point.x > r.xMax)
                r.xMax = point.x;
            
            if (point.x < r.xMin)
                r.xMin = point.x;
            
            if (point.y > r.yMax)
                r.yMax = point.y;
            
            if (point.y < r.yMin)
                r.yMin = point.y;
            return r;
        }

        public static Rect BeginList(this Rect listRect, int elementCount)
        {
            if (!listRect.IsValid() || elementCount <= 1)
                return listRect;
            float elementHeight = listRect.height / elementCount;
            Rect elementRect = listRect.SetHeight(elementHeight);
            return elementRect;
        }

        public static Rect MoveNext(this Rect elementRect, Rect listRect)
        {
            elementRect.y += elementRect.height;
            if (elementRect.yMax > listRect.yMax)
                elementRect.yMax = listRect.yMax;
            return elementRect;
        }
    }
}