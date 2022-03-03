using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rhinox.Lightspeed.IO
{
    public static partial class FileHelper
    {
        public static void ClearDirectoryContentsIfExists(string path)
        {
            DirectoryInfo  di = new DirectoryInfo(path);
            if (!di.Exists)
                return;
            foreach (FileInfo file in di.EnumerateFiles())
                file.Delete(); 
            
            foreach (DirectoryInfo dir in di.EnumerateDirectories())
                dir.Delete(true); 
        }

        public static void DeleteDirectoryIfExists(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
                return;
            di.Delete(true); 
        }
        
        public static bool IsDirectoryEmpty(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
                return true;
            return di.GetFiles().Length == 0 && di.GetDirectories().Length == 0;
        }
        
        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="path">Contains the path that defines the endpoint of the relative path.</param>
        /// <param name="parentPath">Contains the directory that defines the start of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parentPath"/> or <paramref name="path"/> is <c>null</c>.</exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetRelativePath(string path, string parentPath)
        {
            if (string.IsNullOrEmpty(parentPath))
                throw new ArgumentNullException(nameof(parentPath));

            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            // Normalize path separators to Path.DirectorySeparatorChar
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            parentPath = parentPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            
            if (IsPathEqual(path, parentPath, false))
                return string.Empty;
            
            Uri fromUri = new Uri(AppendDirectorySeparatorChar(parentPath)); // Should always be a folder
            Uri toUri = new Uri(path); // Can be a file or folder path

            if (fromUri.Scheme != toUri.Scheme) // Does not have same root
                return path;

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static bool IsPathEqual(string path1, string path2, bool convertSlashes = true)
        {
            if (path1 == null || path2 == null)
                return path1 == path2;
            if (convertSlashes)
            {
                // Normalize path separators to Path.DirectorySeparatorChar
                path1 = path1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                path2 = path2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            if (path1.EndsWith(Path.DirectorySeparatorChar.ToString()))
                path1 = path1.Substring(0, path1.Length - 1);
            if (path2.EndsWith(Path.DirectorySeparatorChar.ToString()))
                path2 = path2.Substring(0, path2.Length - 1);
            return path1.Equals(path2);
        }
        
        public static bool IsPathRooted(string path)
        {
            if (IsFileUrl(path)) return true;
            return Path.IsPathRooted(path);
        }
        
        public static bool IsFileUrl(string path)
        {
            return path.StartsWithOneOf("jar:", "file://", "http://", "https://");
        }
        
        public static string GetFullPath(string filePath, string parentPath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) 
                return filePath;

            if (IsPathRooted(filePath))
                return filePath;
            string path = Path.Combine(parentPath, filePath);
            return new DirectoryInfo(path).FullName;
        }
        
        public static string GetFullFilePath(string filePath, string parentPath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) 
                return filePath;

            if (IsPathRooted(filePath))
                return filePath;
            string path = Path.Combine(parentPath, filePath);
            return new FileInfo(path).FullName;
        }
        
        public static void CopyDirectory(string source, string target, bool overwrite = false)
        {
            var stack = new Stack<Folders>();
            stack.Push(new Folders(source, target));

            while (stack.Count > 0)
            {
                var folders = stack.Pop();
                Directory.CreateDirectory(folders.Target);
                foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                {
                    string destFileName = Path.Combine(folders.Target, Path.GetFileName(file));
                    File.Copy(file, destFileName, overwrite);
                }

                foreach (var folder in Directory.GetDirectories(folders.Source))
                {
                    stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                }
            }
        }

        public class Folders
        {
            public string Source { get; private set; }
            public string Target { get; private set; }

            public Folders(string source, string target)
            {
                Source = source;
                Target = target;
            }
        }
        
        //Combine multiple paths using Path.Combine
        public static string Combine(params string[] components)
        {
            if (components.Length < 1)
            {
                throw new ArgumentException("At least one component must be provided!");
            }

            string str = components[0];
            for (int i = 1; i < components.Length; i++)
            {
                str = Path.Combine(str, components[i]);
            }

            return str;
        }
        
        public static ICollection<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            var di = new DirectoryInfo(path);
            if (!di.Exists)
                return Array.Empty<string>();
            
            var foldersToProcess = new List<string>() { path };
            var result = new List<string>();
            while (foldersToProcess.Count > 0)
            {
                string folder = foldersToProcess[0];
                foldersToProcess.RemoveAt(0);
        
                if (searchOption.HasFlag(SearchOption.AllDirectories))
                {
                    //get subfolders
                    try
                    {
                        var subfolders = Directory.GetDirectories(folder);
                        foldersToProcess.AddRange(subfolders);
                    }
                    catch (Exception ex)
                    {
                        //log if you're interested
                    }
                }
        
                //get files
                var files = new List<string>();
                try
                {
                    files = Directory.GetFiles(folder, searchPattern, SearchOption.TopDirectoryOnly).ToList();
                }
                catch (Exception ex)
                {
                    //log if you're interested
                }
        
                foreach (var file in files)
                {
                    if (string.IsNullOrWhiteSpace(file))
                        continue;
                    result.Add(file);
                }
            }

            return result;
        }
        
        private static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }

        public static IReadOnlyCollection<DirectoryInfo> GetChildFolders(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
                return Array.Empty<DirectoryInfo>();

            return di.GetDirectories();
        }

        public static bool HasExtension(this string path, string extension) // ".fbx"
        {
            string pathExt = Path.GetExtension(path);
            if (pathExt == null)
                return extension == null;
            return pathExt.Equals(extension, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string StripLastFolder(string s, bool separatorBackSlash = true, bool forceEndWithDirectorySeparator = false)
        {
            char targetSeparator = separatorBackSlash ? Path.DirectorySeparatorChar : Path.AltDirectorySeparatorChar;
            char otherSeparator = separatorBackSlash ? Path.AltDirectorySeparatorChar : Path.DirectorySeparatorChar;
            
            
            string path = s.Replace(otherSeparator, targetSeparator); // Set all to \\
            
            string[] parts = path.Split(targetSeparator).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            parts = parts.Take(parts.Length - 1).ToArray();
            
            string result = string.Join(targetSeparator.ToString(), parts);
            if (forceEndWithDirectorySeparator)
                result += targetSeparator;
            return result;
        }
    }
}