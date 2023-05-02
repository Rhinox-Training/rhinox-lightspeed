using System;
using System.Threading;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static class TextureExtensions
    {
        public static Texture2D Pad(this Texture2D tex, int padding)
            => Pad(tex, new RectOffset(padding, padding, padding, padding));
        
        public static Texture2D Pad(this Texture2D tex, Vector2Int padding)
            => Pad(tex, new RectOffset(padding.x, padding.y, padding.x, padding.y));

        public static Texture2D Pad(this Texture2D tex, RectOffset padding)
        {
            if (tex == null)
                return tex;
            
            var paddedTex = tex.CreatePadded(padding);

            // Not sure WHY this is needed but unfortunately the color does not properly scale (?)
            InsetBorder(paddedTex, padding, Color.clear);

            paddedTex.Apply(true);
            return paddedTex;
        }

        public static Texture2D CreatePadded(this Texture2D tex, RectOffset padding)
        {
            var paddedTex = CreateTextureBasedOn(tex);

#if UNITY_2021_2_OR_NEWER
            paddedTex.Reinitialize(tex.width + padding.left + padding.right, tex.height + padding.top + padding.bottom);
#else
            paddedTex.Resize(tex.width + padding.left + padding.right, tex.height + padding.top + padding.bottom);
#endif
            paddedTex.Apply();
            
            Graphics.CopyTexture(
                tex, 0, 0, 0, 0, tex.width, tex.height,
                paddedTex, 0, 0, padding.left, padding.top
            );
            
            return paddedTex;
        }

        private static Texture2D CreateTextureBasedOn(Texture2D tex)
        {
            Texture2D paddedTex = new Texture2D(1, 1, tex.format, tex.mipmapCount > 0)
            {
#if UNITY_EDITOR
                alphaIsTransparency = tex.alphaIsTransparency,
#endif
                wrapMode = tex.wrapMode,
                wrapModeU = tex.wrapModeU,
                wrapModeV = tex.wrapModeV,
                wrapModeW = tex.wrapModeW,
                filterMode = tex.filterMode,
                anisoLevel = tex.anisoLevel
            };
            
            var color = Color.clear;
            paddedTex.SetPixel(0, 0, color);
            paddedTex.Apply(false);
            return paddedTex;
        }

        public static Texture2D MakeSquare(this Texture2D tex)
        {
            float halfDiff = Mathf.Abs(tex.width - tex.height) / 2f;
            RectOffset offset;
            if (tex.width > tex.height)
                offset = new RectOffset(0, 0, Mathf.CeilToInt(halfDiff), Mathf.FloorToInt(halfDiff));
            else
                offset = new RectOffset(Mathf.CeilToInt(halfDiff), Mathf.FloorToInt(halfDiff), 0, 0);
            
            return tex.Pad(offset);
        }
        
        public static Texture2D MakeSquare(this Texture2D tex, int size)
        {
            var newTex = tex.MakeSquare();
#if UNITY_2021_2_OR_NEWER
            TextureScale.Bilinear(newTex, size, size);
#else
            newTex.Resize(size, size);
#endif
            return newTex;
        }

        public static void InsetBorder(this Texture2D tex, RectOffset border, Color color)
        {
            for (int x = 0; x < border.left; ++x)
            {
                for (int y = 0; y < tex.height; ++y)
                    tex.SetPixel(x, y, color);
            }

            for (int x = 0; x < border.right; ++x)
            {
                for (int y = 0; y < tex.height; ++y)
                    tex.SetPixel(tex.width - 1 - x, y, color);
            }

            for (int y = 0; y < border.bottom; ++y)
            {
                for (int x = border.left; x < tex.width - border.right; ++x)
                    tex.SetPixel(x, y, color);
            }

            for (int y = 0; y < border.top; ++y)
            {
                for (int x = border.left; x < tex.width - border.right; ++x)
                    tex.SetPixel(x, tex.height - 1 - y, color);
            }
        }
    }
}