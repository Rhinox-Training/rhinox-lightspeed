using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rhinox.Lightspeed
{
    public static class StringExtensions
    {
        /// <summary>
        /// [ExtensionMethod] Contains any of the given
        /// </summary>
        public static bool ContainsOneOf(this string first, params string[] others)
        {
            return others.Any(second => first.Contains(second));
        }
        
        /// <summary>
        /// [ExtensionMethod] Contains string with given comparisonType
        /// </summary>
        public static bool Contains(this string str, string toCheck, StringComparison comparisonType)
        {
            return str.IndexOf(toCheck, comparisonType) >= 0;
        }

        /// <summary>
        /// [ExtensionMethod] Equals method but with any of the given
        /// </summary>
        public static bool StartsWithOneOf(this string first, params string[] others)
        {
            return others.Any(second => first.StartsWith(second));
        }

        /// <summary>
        /// [ExtensionMethod] Equals method but with any of the given
        /// </summary>
        public static bool EndsWithOneOf(this string first, params string[] others)
        {
            return others.Any(second => first.EndsWith(second));
        }
        
        /// <summary>
        /// [ExtensionMethod] Remove the first occurence of the given value
        /// </summary>
        public static string RemoveFirst(this string str, string value, StringComparison comparison = StringComparison.InvariantCulture)
        {
            int startI = str.IndexOf(value, comparison);

            if (startI == -1)
                return str;

            return str.Remove(startI, value.Length);
        }
        
        public static string Replace(this string str, int index, int length, string replacement)
        {
            return str.Remove(index, length).Insert(index, replacement);
        }

        /// <summary>
        /// [ExtensionMethod] Replace the first occurence of the given value with the given string
        /// </summary>
        public static string ReplaceFirst(this string str, string oldValue, string newValue, StringComparison comparison = StringComparison.InvariantCulture)
        {
            int startI = str.IndexOf(oldValue, comparison);

            if (startI == -1)
                return str;

            return str.Remove(startI, oldValue.Length).Insert(startI, newValue);
        }
        
        /// <summary>
        /// [ExtensionMethod] Remove the last occurence of the given value
        /// </summary>
        public static string RemoveLast(this string str, string value, StringComparison comparison = StringComparison.InvariantCulture)
        {
            int startI = str.LastIndexOf(value, comparison);

            if (startI == -1)
                return str;

            return str.Remove(startI, value.Length);
        }
        
        /// <summary>
        /// [ExtensionMethod] Replace the last occurence of the given value with the given string
        /// </summary>
        public static string ReplaceLast(this string str, string oldValue, string newValue, StringComparison comparison = StringComparison.InvariantCulture)
        {
            int startI = str.LastIndexOf(oldValue, comparison);

            if (startI == -1)
                return str;

            return str.Remove(startI, oldValue.Length).Insert(startI, newValue);
        }
        
        /// <summary>
        /// [ExtensionMethod] If given string is null or whitespace; use the alternative given string instead
        /// </summary>
        public static string DefaultTo(this string str, string val)
        {
            return string.IsNullOrWhiteSpace(str) ? val : str;
        }
        
        /// <summary>
        /// [ExtensionMethod] Get the part that all given strings start with
        /// </summary>
        public static string GetCommonPrefix(this ICollection<string> strings)
        {
            if (strings == null || strings.Count == 0) return string.Empty;
            
            var candidates = strings.First().Substring(0, strings.Min(s => s.Length));
            var result = candidates
                .TakeWhile((c, i) => strings.All(s => s[i] == c))
                .ToArray();
            
            return new string(result);
        }
        
        /// <summary>
        /// [ExtensionMethod] Get the part that all given strings end with
        /// </summary>
        public static string GetCommonPostfix(this ICollection<string> strings)
        {
            if (strings == null || strings.Count == 0) return string.Empty;

            var first = strings.First();
            var length = strings.Min(s => s.Length);
            // reverse the string to start at the end
            var candidates = first.Substring(first.Length - length, length).Reverse();
            var result = candidates
                .TakeWhile((c, i) => strings.All(s => s[s.Length - 1 - i] == c))
                .Reverse() // re-revert the string
                .ToArray();
            return new string(result);
        }

        /// <summary>
        /// [ExtensionMethod] Removes invalid chars for a file name/path
        /// </summary>
        public static bool IsFileSystemSafe(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            var invalidChars = Path.GetInvalidPathChars();
            return !str.Any(x => invalidChars.Contains(x));
        }
        
        /// <summary>
        /// [ExtensionMethod] Replaces backslashes with forward slashes
        /// </summary>
        public static string ToLinuxSafePath(this string path)
        {
            //path = path.ToLowerInvariant();
            return path.Replace('\\', '/');
        }
        
        /// <summary>
        /// [ExtensionMethod] Gets a determinate hash for the given input
        /// </summary>
        public static string ToMD5Hash(this string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

        private const string _camelCaseSplitPattern = @"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z]) |(?<=[A-Za-z])(?=[^A-Za-z])";
        private static Regex _camelCaseSplitRegex;
        
        public static string SplitCamelCase(this string input, string separator = " ")
        {
            if (input.IsNullOrEmpty()) return input;
            
            if (_camelCaseSplitRegex == null)
                _camelCaseSplitRegex = new Regex(_camelCaseSplitPattern, RegexOptions.IgnorePatternWhitespace);
            
            return string.Join(separator, _camelCaseSplitRegex.Split(input));
        }
        
        public static bool CaseInvariantEquals(this string s1, string s2)
        {
            if (s1 == null)
                return s2 == null;
            return s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}