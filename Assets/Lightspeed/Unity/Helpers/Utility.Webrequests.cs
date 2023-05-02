using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
#if NEWTONSOFT
using Newtonsoft.Json;
#endif

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static T ParseJsonResult<T>(this UnityWebRequest request, bool useNewtonsoftIfAvailable = false)
        {
            var text = request.downloadHandler.text;
            if (typeof(T) == typeof(string))
                return (T)(object)text;
            return FromJson<T>(text, useNewtonsoftIfAvailable);
        }
        
        public static T FromJson<T>(string o, bool useNewtonsoftIfAvailable = false)
        {
            if (useNewtonsoftIfAvailable)
            {
#if NEWTONSOFT
                // Include Newtonsoft json to support anonymous objects
                return JsonConvert.DeserializeObject<T>(o);
#endif
            }
            return JsonUtility.FromJson<T>(o);
        }
        
        public static string ToJson<T>(T o, bool useNewtonsoftIfAvailable = false)
        {
            if (useNewtonsoftIfAvailable)
            {
#if NEWTONSOFT
                // Include Newtonsoft json to support anonymous objects
                return JsonConvert.SerializeObject(o);
#endif
            }
            return JsonUtility.ToJson(o);
        }
        
        public static bool IsRequestValid(this UnityWebRequest request, out string error)
        {
#if UNITY_2020_1_OR_NEWER
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    error = $"[{request.uri}] Error: {request.error}";
                    return false;
                case UnityWebRequest.Result.ProtocolError:
                    error = $"[{request.uri}] HTTP Error: {request.error}";
                    return false;
                case UnityWebRequest.Result.Success:
                    error = null;
                    // Debug.Log(uri + ":\nReceived: " + request.downloadHandler.text);
                    return true;
            }
            throw new NotImplementedException($"Unknown result type: {request.result}");
#else
            if (request.isHttpError)
            {
                error = $"{request.uri}: HTTP Error: {request.error}";
                return false;
            }
            
            if (request.isNetworkError)
            {
                error = $"[{request.uri}] Error: {request.error}";
                return false;
            }
            
            error = null;
            return true;
#endif
        }

        public static NameValueCollection ExtractQueriesFromUrl(this string url)
            => ExtractQueriesFromUrl(new Uri(url));

        private const string _queryRegexString = @"[\?\&]([^=]+)\=([^&]+)";
        private static Regex _queryRegex;
        public static NameValueCollection ExtractQueriesFromUrl(this Uri uri)
        {
#if UNITY_2021_2
            return HttpUtility.ParseQueryString(uri.Query);
#else
            if (_queryRegex == null)
                _queryRegex = new Regex(_queryRegexString);
            var coll = new NameValueCollection();
            var matches = _queryRegex.Matches(uri.Query);
            for (int i = 0; i < matches.Count; ++i)
            {
                var match = matches[i];
                coll.Add(match.Groups[0].Value, match.Groups[1].Value);
            }
            return coll;
#endif
        }

        public static string[] ExtractSegmentsFromUrl(this string url)
            => ExtractSegmentsFromUrl(new Uri(url));
        
        public static string[] ExtractSegmentsFromUrl(this Uri uri)
            => uri.Segments;
    }
}