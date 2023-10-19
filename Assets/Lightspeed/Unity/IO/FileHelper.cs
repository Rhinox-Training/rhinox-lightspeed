using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rhinox.Lightspeed;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
                return ExecuteSynchronousWebRequest(path, suppressLog: true) != null;
            return File.Exists(path);
        }
        
        public static string ReadAllText(string path, int timeOut = 10, bool suppressLog = false)
        {
            if (!IsFileUrl(path))
                return File.ReadAllText(path);
            
            var response = ExecuteSynchronousWebRequest(path, timeOut);
            if (response == null)
            {
                if (!suppressLog)
                    Debug.LogError($"File at {path} failed to load...");
                return null;
            }
            return response.text;

        }
    
        public static byte[] ReadAllBytes(string path, int timeOut = 10, bool suppressLog = false)
        {
            if (!IsFileUrl(path))
                return File.ReadAllBytes(path);
            
            var response = ExecuteSynchronousWebRequest(path, timeOut);
            if (response == null)
            {
                if (!suppressLog)
                    Debug.LogError($"File at {path} failed to load...");
                return Array.Empty<byte>();
            }
            return response.data;

        }

        public static string[] ReadAllLines(string path, int timeOut = 10, bool suppressLog = false)
        {
            if (!IsFileUrl(path))
                return File.ReadAllLines(path);
            
            var response = ExecuteSynchronousWebRequest(path, timeOut);
            if (response == null)
            {
                if (!suppressLog)
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
        
        public static async Task<byte[]> ReadAllBytesAsync(string filePath, int timeOut = 10, bool suppressLog = false)
        {
            byte[] data = null;
            
            // Check if we should use UnityWebRequest or File.ReadAllBytes
            if (filePath.Contains("://"))
            {
                UnityWebRequest www = UnityWebRequest.Get(filePath);
                www.timeout = timeOut;
                await www.SendWebRequest();

                if (www.IsRequestValid(out string error))
                    data = www.downloadHandler.data;
                else if (!suppressLog)
                    Debug.LogError(error);
            }
            else
            {
#if !UNITY_2021_1_OR_NEWER
                data = File.ReadAllBytes(filePath);
#else
                data = await File.ReadAllBytesAsync(filePath);
#endif
            }

            if (data == null && !suppressLog)
                Debug.LogError("File not found");

            return data;
        }
        
        public static async Task<string[]> ReadAllLinesAsync(string filePath, int timeOut = 10, bool suppressLog = false)
        {
            string[] data = null;
            // Check if we should use UnityWebRequest or File.ReadAllBytes
            if (filePath.Contains("://"))
            {
                UnityWebRequest www = UnityWebRequest.Get(filePath);
                www.timeout = timeOut;
                await www.SendWebRequest();

                if (www.IsRequestValid(out string error))
                    data = www.downloadHandler.text.SplitLines();
                else if (!suppressLog)
                    Debug.LogError($"Network error: {filePath} - {error}");
            }
            else
            {
#if !UNITY_2021_1_OR_NEWER
                data = File.ReadAllLines(filePath);
#else
                data = await File.ReadAllLinesAsync(filePath);
#endif
            }

            if (data == null && !suppressLog)
                Debug.LogError("File not found");

            return data;
        }

        private static DownloadHandler ExecuteSynchronousWebRequest(string path, int timeOut = 10, bool suppressLog = false)
        {
            // path = path.Replace("http://", "file:///");
            // path = path.Replace("https://", "file:///");
            UnityWebRequest www = UnityWebRequest.Get(path);
            www.timeout = timeOut;
            www.SendWebRequest();
            
            while (!www.isDone) { }

            if (www.IsRequestValid(out string error))
                return www.downloadHandler;
            else if (!suppressLog)
                Debug.LogError(error);
            return null;
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
                Debug.Log("CopyDirectoryRecursive: Pattern '" + regExExcludeFilter +
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
                    if (!filtercallback(subdirectorypath))
                        continue;
                    
                    string directoryName = Path.GetFileName(subdirectorypath);
                    CopyDirectoryFiltered(Path.Combine(sourceDir, directoryName), Path.Combine(targetDir, directoryName), overwrite, filtercallback, recursive);
                }
            }
        }
       
#if UNITY_EDITOR
        public static void CreateAssetsDirectory(string directory)
        {
            var directories = directory.Split('\\', '/', Path.PathSeparator);
            var currentPath = string.Empty;
            foreach (var dir in directories)
            {
                currentPath = Path.Combine(currentPath, dir);
                var fullPath = FileHelper.GetFullPath(currentPath, FileHelper.GetProjectPath());
                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    if (!Directory.Exists(fullPath))
                        AssetDatabase.CreateFolder(Path.GetDirectoryName(currentPath), Path.GetFileName(currentPath));
                    else
                    {
                        // TODO: inconsistent database -.-
                    }
                }
                else
                {
                    Directory.CreateDirectory(fullPath);
                    AssetDatabase.Refresh();
                }
            }
        }
        
        public static bool AssetExists(string assetPath)
        {
            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath)))
                return false;
            var fullPath = FileHelper.GetFullPath(assetPath, FileHelper.GetProjectPath());
            return File.Exists(fullPath);
        }

        public static void ClearAssetDirectory(string assetPath) // Rooted at >Assets/..
        {
            if (!AssetDatabase.IsValidFolder(assetPath))
                return;
            
            // Remove dir (recursively)
            var fullPath = FileHelper.GetFullPath(assetPath, FileHelper.GetProjectPath());
            Directory.Delete(fullPath, true);

            // Sync AssetDatabase with the delete operation.
            AssetDatabase.DeleteAsset(assetPath);

            AssetDatabase.Refresh();
        }
#endif

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