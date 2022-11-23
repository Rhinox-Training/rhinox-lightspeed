using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        private static Dictionary<Color, Texture2D> _textureByColor;

        /// <summary>
        /// Copies a texture (readable or not) into CPU memory of a new texture.
        /// Apply needs to be called for it to be usable in GPU.
        /// </summary>
        public static Texture2D CopyTextureCPU(Texture tex, TextureCreationFlags flags = TextureCreationFlags.None)
        {
            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, tex.graphicsFormat);
            var copy = new Texture2D(tex.width, tex.height, tex.graphicsFormat, flags);
            Graphics.Blit(tex, rt);

            var oldRt = RenderTexture.active;
            RenderTexture.active = rt;
            copy.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            RenderTexture.active = oldRt;
            RenderTexture.ReleaseTemporary(rt);

            return copy;
        }
        
        public static Texture2D GetColorTexture(Color color)
        {
            if (_textureByColor == null)
                _textureByColor = new Dictionary<Color, Texture2D>();

            if (!_textureByColor.ContainsKey(color))
            {
                var pixelTex = new Texture2D(1, 1);
                pixelTex.SetPixel(0, 0, color);
                pixelTex.Apply();
                _textureByColor[color] = pixelTex;
            }

            return _textureByColor[color];
        }

        public static Texture2D Pad(this Texture2D tex, int padding)
            => Pad(tex, new RectOffset(padding, padding, padding, padding));
        
        public static Texture2D Pad(this Texture2D tex, Vector2Int padding)
            => Pad(tex, new RectOffset(padding.x, padding.y, padding.x, padding.y));

        public static Texture2D Pad(this Texture2D tex, RectOffset padding)
        {
            if (tex == null)
                return tex;
            
            Texture2D paddedTex = new Texture2D(1, 1, tex.format, tex.mipmapCount > 0) {
#if UNITY_EDITOR
                alphaIsTransparency = tex.alphaIsTransparency,
#endif
            };

            var color = Color.clear;
            paddedTex.SetPixel(0, 0, color);
            paddedTex.Resize(tex.width + padding.left + padding.right, tex.height + padding.top + padding.bottom);
            paddedTex.Apply();
            
            Graphics.CopyTexture(
                tex, 0, 0, 0, 0, tex.width, tex.height,
                paddedTex, 0, 0, padding.left, padding.top
            );

            // Not sure WHY this is needed but unfortunately the color does not properly scale (?)
            InsetBorder(paddedTex, padding, color);

            paddedTex.Apply(true);
            return paddedTex;
        }

        public static void InsetBorder(this Texture2D tex, RectOffset border, Color color)
        {
            for (int x = 0; x < border.left; ++x)
            {
                for (int y = 0; y < tex.width; ++y)
                    tex.SetPixel(x, y, color);
            }

            for (int x = 0; x < border.right; ++x)
            {
                for (int y = 0; y < tex.width; ++y)
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