using System;
using UnityEngine;

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
    }
}