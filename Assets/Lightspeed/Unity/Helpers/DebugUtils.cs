using System;
using UnityEngine;
// TODO: what version no longer uses namespace Experimental for: TextureCreationFlags
using UnityEngine.Experimental.Rendering;

namespace Rhinox.Lightspeed
{
    public static class DebugUtils
    {
        // Only useful for textures that are not readable
        public static void RenderTextureToPng(Texture2D tex, out string b64)
        {
            var rt = new RenderTexture(tex.width, tex.height, 0);
            Graphics.Blit(tex, rt);

            RenderTextureToPng(rt, out b64);
        }
        
        public static void CopyTexture(Texture2D tex, out Texture2D outputTexture)
        {
            var rt = new RenderTexture(tex.width, tex.height, 0, tex.graphicsFormat);
            Graphics.Blit(tex, rt);

            RenderTextureToTexture2D(rt, out outputTexture);
        }

        public static void RenderTextureToPng(RenderTexture rt, out string b64)
        {
            var oldRT = RenderTexture.active;

            try
            {
                var tex = new Texture2D(rt.width, rt.height);
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();

                var png = tex.EncodeToPNG();
                b64 = Convert.ToBase64String(png);
            }
            finally // just some backup so the RenderTexture.active definitely does not change
            {
                RenderTexture.active = oldRT;
            }
        }
        
        public static void RenderTextureToTexture2D(RenderTexture rt, out Texture2D outputTex)
        {
            var oldRT = RenderTexture.active;

            try
            {
                var tex = new Texture2D(rt.width, rt.height, rt.graphicsFormat, TextureCreationFlags.None);
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();

                outputTex = tex;
            }
            finally // just some backup so the RenderTexture.active definitely does not change
            {
                RenderTexture.active = oldRT;
            }
        }
    }
}