using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Rhinox.Utilities
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
            var pathUri = new Uri(path);
            return ApplicationFolderUri.MakeRelativeUri(pathUri).ToString();
        }

        /// <summary>
        /// Get a valid Asset path from a 'Libarary/PackageCache/' path
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
    }
}