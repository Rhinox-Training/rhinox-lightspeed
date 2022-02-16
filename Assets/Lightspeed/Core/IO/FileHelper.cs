using System;
using System.Collections.Generic;
using System.IO;

namespace Rhinox.Lightspeed.IO
{
    public static class FileHelper
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
        
        public static string GetRelativePath(string filePath, string parentPath)
        {
            Uri pathUri = new Uri(filePath);
            // Folders must end in a slash
            if (!parentPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                parentPath += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(parentPath);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
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
        
        public static void CopyDirectory(string source, string target)
        {
            var stack = new Stack<Folders>();
            stack.Push(new Folders(source, target));

            while (stack.Count > 0)
            {
                var folders = stack.Pop();
                Directory.CreateDirectory(folders.Target);
                foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                {
                    File.Copy(file, Path.Combine(folders.Target, Path.GetFileName(file)));
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
    }
}