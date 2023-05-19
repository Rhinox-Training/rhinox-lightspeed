using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        static Uri _packageCacheUri;
        static Uri PackageCacheUri => _packageCacheUri ?? (_packageCacheUri = new Uri(@"Library/PackageCache/", UriKind.Relative));
        
        static Uri _packagesUri;
        static Uri PackagesUri => _packagesUri ?? (_packagesUri = new Uri(@"Packages/", UriKind.Relative));

        static Uri _applicationFolderUri;
        static Uri ApplicationFolderUri => _applicationFolderUri ?? (_applicationFolderUri = new Uri(Directory.GetParent(Application.dataPath).FullName + '/'));

        const string PackageCacheRegexStr = @"Library[\/\\]PackageCache[\/\\](.*?)@(?:.*?)[\/\\](.*)";
        static Regex _packageCacheRegex;
        static Regex PackageCacheRegex => _packageCacheRegex ?? (_packageCacheRegex = new Regex(PackageCacheRegexStr, RegexOptions.Compiled | RegexOptions.IgnoreCase)); 
        
        public static string GetProjectFolder()
        {
            return ApplicationFolderUri.AbsolutePath;
        }

        public static string GetPathRelativeToProject(string path)
        {
            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(path);
            var pathUri = new Uri(path);
            return ApplicationFolderUri.MakeRelativeUri(pathUri).ToString();
        }

        /// <summary>
        /// Get a valid Asset path from a 'Library/PackageCache/' path
        /// </summary>
        public static string ParsePackagePath(string absolutePath)
        {
            var relPath = GetPathRelativeToProject(absolutePath);
            var match = PackageCacheRegex.Match(relPath);
            if (!match.Success) return string.Empty;
            return $"Packages/{match.Groups[1]}/{match.Groups[2]}";
        }

        /// <summary>
        /// Check whether the path is a 'Libarary/PackageCache/' path.
        /// Returns false when it is a 'Packages/' path
        /// </summary>
        public static bool IsPackagePath(string path) => IsPackagePath(path, out _);
        
        public static bool IsPackagePath(string path, out bool isEditable)
        {
            Uri uri = new Uri(path);
            Uri relUri = ApplicationFolderUri.MakeRelativeUri(uri);

            bool isPackage = relUri.OriginalString.StartsWith(PackageCacheUri.OriginalString);

            if (relUri.OriginalString.StartsWith(PackagesUri.OriginalString))
            {
                isPackage = true;
                isEditable = true;
            }
            else isEditable = false;
            
            return isPackage;
        }
        
#if UNITY_EDITOR
        public static ICollection<PackageInfo> ListPackages(bool includeUnityPackages = false)
        {
            const string STANDARD_UNITY_PACKAGE_PREFIX = "com.unity.";
            ListRequest lr = Client.List(true, false);
            while (!lr.IsCompleted)
            {
                // Wait
            }

            List<PackageInfo> packages = new List<PackageInfo>();
            foreach (PackageInfo package in lr.Result)
            {
                string packageName = package.name;
                if (!includeUnityPackages && packageName.StartsWith(STANDARD_UNITY_PACKAGE_PREFIX))
                    continue;
                packages.Add(package);
            }

            return packages;
        }
#endif
    }
}