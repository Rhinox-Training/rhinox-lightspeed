using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Rhinox.Lightspeed;
using UnityEngine;
using UnityEngine.Networking;

namespace Rhinox.Lightspeed.IO
{
    public static partial class FileHelper
    {
    #if UNITY_EDITOR
        /// <summary>
        /// Absolute path to project
        /// </summary>
        public static string GetProjectPath()
        {
            var directoryInfo  = new DirectoryInfo(Application.dataPath);
            return directoryInfo.Parent.FullName;
        }
    #endif

        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!DirectoryExists(path))
                Directory.CreateDirectory(path);
        }

        public static bool DirectoryExists(string path)
        {
            if (IsFileUrl(path))
            {
                return ExecuteSynchronousWebRequest(path, suppressLog: true) != null;
            }
            
            DirectoryInfo di = new DirectoryInfo(path);
            return di.Exists;
        }
        
        public static bool Exists(string path)
        {
            if (IsFileUrl(path))
            {
                return ExecuteSynchronousWebRequest(path, suppressLog: true) != null;
                // var www = UnityWebRequest.Get(path);
                // www.timeout = 10;
                // www.SendWebRequest();
                // while (!www.isDone) {}
                // return !(www.isNetworkError || www.isHttpError);
            }
            return File.Exists(path);
        }
        
        public static string ReadAllText(string path, int timeOut = 10)
        {
            if (IsFileUrl(path))
            {
                var response = ExecuteSynchronousWebRequest(path, timeOut);
                if (response == null)
                {
                    Debug.LogError($"File at {path} failed to load...");
                    return null;
                }
                return response.text;
            }
            return File.ReadAllText(path);

        }
    
        public static byte[] ReadAllBytes(string path, int timeOut = 10)
        {
            if (IsFileUrl(path))
            {
                var response = ExecuteSynchronousWebRequest(path, timeOut);
                if (response == null)
                {
                    Debug.LogError($"File at {path} failed to load...");
                    return Array.Empty<byte>();
                }
                return response.data;
            }
            return File.ReadAllBytes(path);

        }

        public static string[] ReadAllLines(string path, int timeOut = 10)
        {
            if (IsFileUrl(path))
            {
                var response = ExecuteSynchronousWebRequest(path, timeOut);
                if (response == null)
                {
                    Debug.LogError($"File at {path} failed to load...");
                    return Array.Empty<string>();
                }
                
                var reader = new StringReader(response.text);
                var list = new List<string>();
                while (reader.Peek() >= 0)
                {
                    list.Add(reader.ReadLine());
                }
			    return list.ToArray();
            }
            return File.ReadAllLines(path);

        }
        
        public static IEnumerator ReadAllBytesAsync(string filePath, Action<string, byte[]> callback)
        {
            byte[] data = null;
            // Check if we should use UnityWebRequest or File.ReadAllBytes
            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                UnityWebRequest www = UnityWebRequest.Get(filePath);
                www.timeout = 10;
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogError($"Network error: {filePath} - {www.error}");
                    data = null;
                }
                else
                    data = www.downloadHandler.data;
            }
            else
            {
                data = File.ReadAllBytes(filePath);
            }

            if (data == null)
            {
                Debug.LogError("File not found");
                yield break;
            }

            callback?.Invoke(filePath, data);
        }
        
        public static IEnumerator ReadAllLinesAsync(string filePath, Action<string, string[]> callback)
        {
            string[] data = null;
            // Check if we should use UnityWebRequest or File.ReadAllBytes
            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                UnityWebRequest www = UnityWebRequest.Get(filePath);
                www.timeout = 10;
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogError($"Network error: {filePath} - {www.error}");
                    data = null;
                }
                else
                {
                    var reader = new StringReader(www.downloadHandler.text);
                    var list = new List<string>();
                    while (reader.Peek() >= 0)
                    {
                        list.Add(reader.ReadLine());
                    }
                    data = list.ToArray();
                }
            }
            else
            {
                data = File.ReadAllLines(filePath);
            }

            if (data == null)
            {
                Debug.LogError("File not found");
                yield break;
            }

            callback?.Invoke(filePath, data);
        }

        private static DownloadHandler ExecuteSynchronousWebRequest(string path, int timeOut = 10, bool suppressLog = false)
        {
            // path = path.Replace("http://", "file:///");
            // path = path.Replace("https://", "file:///");
            UnityWebRequest www = UnityWebRequest.Get(path);
            www.timeout = timeOut;
            www.SendWebRequest();
            
            while (!www.isDone) { }
            
            if (www.isNetworkError || www.isHttpError)
            {
                if (!suppressLog)
                    Debug.LogError($"Network error: {path} - {www.error}");
                return null;
            }

            return www.downloadHandler;
        }
        
        public static void CreateOrCleanDirectory(string dir)
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);

            Directory.CreateDirectory(dir);
        }
        
        // Fix forward slashes on other platforms than windows, for example build server on Unix
        private static string FixForwardSlashes(string unityPath)
        {
            return ((Application.platform != RuntimePlatform.WindowsEditor) ? unityPath : unityPath.Replace("/", @"\"));
        }

        // Copies the contents of one directory to another.
        public static void CopyDirectoryFiltered(string source, string target, bool overwrite, string regExExcludeFilter, bool recursive)
        {
            RegexMatcher excluder = new RegexMatcher()
            {
                exclude = null
            };
            try
            {
                if (regExExcludeFilter != null)
                {
                    excluder.exclude = new Regex(regExExcludeFilter);
                }
            }
            catch (ArgumentException)
            {
                UnityEngine.Debug.Log("CopyDirectoryRecursive: Pattern '" + regExExcludeFilter +
                                      "' is not a correct Regular Expression. Not excluding any files.");
                return;
            }

            CopyDirectoryFiltered(source, target, overwrite, excluder.CheckInclude, recursive);
        }

        public static void CopyDirectoryFiltered(string sourceDir, string targetDir, bool overwrite, Func<string, bool> filtercallback, bool recursive)
        {
            // Create directory if needed
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
                overwrite = false;
            }

            // Iterate all files, files that match filter are copied.
            foreach (string filepath in Directory.GetFiles(sourceDir))
            {
                if (filtercallback(filepath))
                {
                    string fileName = Path.GetFileName(filepath);
                    string to = Path.Combine(targetDir, fileName);


                    File.Copy(FixForwardSlashes(filepath), FixForwardSlashes(to), overwrite);
                }
            }

            // Go into sub directories
            if (recursive)
            {
                foreach (string subdirectorypath in Directory.GetDirectories(sourceDir))
                {
                    if (filtercallback(subdirectorypath))
                    {
                        string directoryName = Path.GetFileName(subdirectorypath);
                        CopyDirectoryFiltered(Path.Combine(sourceDir, directoryName), Path.Combine(targetDir, directoryName), overwrite, filtercallback, recursive);
                    }
                }
            }
        }

        internal struct RegexMatcher
        {
            public Regex exclude;

            public bool CheckInclude(string s)
            {
                return exclude == null || !exclude.IsMatch(s);
            }
        }
    }
}