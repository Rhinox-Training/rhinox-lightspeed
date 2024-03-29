using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rhinox.Lightspeed.IO
{
    public static partial class FileHelper
    {
        /// <summary>
        /// Absolute path to project
        /// </summary>
        public static string GetProjectPath()
        {
            var directoryInfo  = new DirectoryInfo(Application.dataPath);
            return directoryInfo.Parent.FullName;
        }

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
        
        public static async Task<string> ReadAllTextAsync(string path, int timeOut = 10, bool suppressLog = false)
        {
            if (!IsFileUrl(path))
#if !UNITY_2021_1_OR_NEWER
                return File.ReadAllText(path);
#else
                return await File.ReadAllTextAsync(path);
#endif
            
            var response = await ExecuteWebRequestAsync(path, timeOut);
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
        
        public static async Task<byte[]> ReadAllBytesAsync(string path, int timeOut = 10, bool suppressLog = false)
        {
            if (!IsFileUrl(path))
            {
#if !UNITY_2021_1_OR_NEWER
                return File.ReadAllBytes(path);
#else
                return await File.ReadAllBytesAsync(path);
#endif
            }
            
            var response = await ExecuteWebRequestAsync(path, timeOut);
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
        
        public static async Task<string[]> ReadAllLinesAsync(string path, int timeOut = 10, bool suppressLog = false)
        {
            if (!IsFileUrl(path))
            {
#if !UNITY_2021_1_OR_NEWER
                return File.ReadAllLines(path);
#else
                return await File.ReadAllLinesAsync(path);
#endif
            }
            
            var response = await ExecuteWebRequestAsync(path, timeOut, suppressLog);
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
#if !UNITY_2021_1_OR_NEWER
                list.Add(reader.ReadLine());
#else
                list.Add(await reader.ReadLineAsync());
#endif
            }
            return list.ToArray();
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

        private static async Task<DownloadHandler> ExecuteWebRequestAsync(string path, int timeOut = 10, bool suppressLog = false)
        {
            UnityWebRequest www = UnityWebRequest.Get(path);
            www.timeout = timeOut;
            
            await www.SendWebRequest();
            
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
            RegexMatcher excluder = new RegexMatcher();
            try
            {
                if (regExExcludeFilter != null)
                    excluder.Exclude = new Regex(regExExcludeFilter);
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
            if (!recursive) return;
            
            foreach (string subdirectorypath in Directory.GetDirectories(sourceDir))
            {
                if (!filtercallback(subdirectorypath))
                    continue;
                    
                string directoryName = Path.GetFileName(subdirectorypath);
                CopyDirectoryFiltered(Path.Combine(sourceDir, directoryName), Path.Combine(targetDir, directoryName), overwrite, filtercallback, recursive);
            }
        }
       
#if UNITY_EDITOR
        public static bool MoveAsset(string assetPath, string newPath)
        {
            if (!File.Exists(assetPath))
                return false;
            
            var directory = Path.GetDirectoryName(newPath);
            FileHelper.CreateAssetsDirectory(directory);
            var error = AssetDatabase.MoveAsset(assetPath, newPath);
            if (error.IsNullOrEmpty())
                return true;
            
            Debug.LogError($"Failed to move asset '{assetPath}': {error}");
            return false;
        }
        
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
            public Regex Exclude;

            public bool CheckInclude(string s)
            {
                return Exclude == null || !Exclude.IsMatch(s);
            }
        }
    }
}