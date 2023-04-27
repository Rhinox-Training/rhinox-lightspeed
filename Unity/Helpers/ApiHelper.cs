using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rhinox.Lightspeed;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Helpers
{
    public class ApiHelper
    {
        private string _baseUrl;
        
        public ApiHelper(string url)
        {
            _baseUrl = url;
        }
        
        public delegate void WebRequestAction(UnityWebRequest request);

        public async Task Get(string path, WebRequestAction handleRequest)
        {
            string uri = $"{_baseUrl}{path}";
            using (var request = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                await request.SendWebRequest();
                
                if (HandleResult(request))
                    handleRequest?.Invoke(request);
            }
        }

        public async Task<T> Get<T>(string path)
        {
            string uri = $"{_baseUrl}{path}";
            using (var request = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                await request.SendWebRequest();

                if (HandleResult(request))
                    return FromJsonResult<T>(request);
                
                return default;
            }
        }

        public T GetSync<T>(string path)
        {
            string uri = $"{_baseUrl}{path}";
            using (var request = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                var op = request.SendWebRequest();
                while (!op.isDone)
                {
                }

                return FromJsonResult<T>(request);
            }
        }

        public async Task<TResult> Post<TResult>(string path, object o)
        {
            var json = JsonConvert.SerializeObject(o);
            var result = await Post<TResult>(path, json);
            return result;
        }

        public async Task Post(string path, object o, WebRequestAction handleRequest = null)
        {
            var json = JsonConvert.SerializeObject(o);
            await Post(path, json, handleRequest);
        }

        public async Task Post(string path, string json, WebRequestAction handleRequest = null)
        {
            string uri = $"{_baseUrl}{path}";
            using (var request = new UnityWebRequest(uri, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                // Request and wait for the desired page.
                await request.SendWebRequest();

                if (HandleResult(request))
                    handleRequest?.Invoke(request);
            }
        }

        public async Task<T> Post<T>(string path, string json)
        {
            string uri = $"{_baseUrl}{path}";
            using (var request = new UnityWebRequest(uri, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                // Request and wait for the desired page.
                await request.SendWebRequest();

                return FromJsonResult<T>(request);
            }
        }

        public TResult PostSync<TResult>(string path, object o)
        {
            var json = JsonConvert.SerializeObject(o);
            return PostSync<TResult>(path, json);
        }

        public TResult PostSync<TResult>(string path, string json)
        {
            string uri = $"{_baseUrl}{path}";
            using (var request = new UnityWebRequest(uri, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                // Request and wait for the desired page.
                var op = request.SendWebRequest();
                while (!op.isDone) { }

                return FromJsonResult<TResult>(request);
            }
        }

        private static bool HandleResult(UnityWebRequest request)
        {
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(request.uri + ": Error: " + request.error);
                    return false;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(request.uri + ": HTTP Error: " + request.error);
                    return false;
                case UnityWebRequest.Result.Success:
                    // Debug.Log(uri + ":\nReceived: " + request.downloadHandler.text);
                    return true;
            }

            throw new NotImplementedException($"Unknown result type: {request.result}");
        }

        private static T FromJsonResult<T>(UnityWebRequest request)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)request.downloadHandler.text;
            return JsonUtility.FromJson<T>(request.downloadHandler.text);
        }
    }
}