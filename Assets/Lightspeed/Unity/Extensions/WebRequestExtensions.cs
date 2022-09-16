using UnityEngine.Networking;

namespace Rhinox.Lightspeed
{
    public static class WebRequestExtensions
    {
        private static void SetContentTypeJson(this UnityWebRequest www)
        {
            www.SetRequestHeader("Content-Type", "application/json");
        }
    }
}