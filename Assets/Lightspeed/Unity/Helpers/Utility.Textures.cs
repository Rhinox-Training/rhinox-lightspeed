using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        /// <summary>
        /// Copies a texture (readable or not) into CPU memory of a new texture.
        /// Apply needs to be called for it to be usable in GPU.
        /// </summary>
        public static Texture2D CopyTextureCPU(Texture tex, TextureCreationFlags flags = TextureCreationFlags.None)
        {
            var oldRt = RenderTexture.active;
            
            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, tex.graphicsFormat);
            var copy = new Texture2D(tex.width, tex.height, tex.graphicsFormat, flags);
            
            Graphics.Blit(tex, rt);
            RenderTexture.active = rt;
            copy.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            
            RenderTexture.active = oldRt;
            RenderTexture.ReleaseTemporary(rt);

            return copy;
        }
        
        private static Dictionary<Color, Texture2D> _textureByColor;

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
    }
}