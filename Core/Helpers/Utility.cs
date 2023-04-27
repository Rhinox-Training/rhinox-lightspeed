using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static string GetCommandLineArg(string name, string defaultValue = null)
        {
            if (TryGetCommandLineArg(out string value, name))
                return value;
            return defaultValue;
        }
        
        public static string GetCommandLineArg(string[] possibleNames, string defaultValue = null)
        {
            if (TryGetCommandLineArg(out string value, possibleNames))
                return value;
            return defaultValue;
        }

        public static bool TryGetCommandLineArg(out string value, params string[] possibleNames)
        {
            var args = System.Environment.GetCommandLineArgs();
            int index = -1;
            
            for (var i = 0; i < possibleNames.Length; i++)
            {
                var name = possibleNames[i];
                if (!name.StartsWith("-"))
                    name = "-" + name;
                index = args.IndexOf(name);
                if (index >= 0) break;
            }

            if (index >= 0 && index < args.Length - 1)
            {
                value = args[index + 1];
                return true;
            }

            value = null;
            return false;
        }

        public static string[] SplitLines(this string input)
        {
            return input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }
        
        public static string GetLongestCommonPrefix(string[] s)
        {
            int k = s[0].Length;
            for (int i = 1; i < s.Length; ++i)
            {
                k = Math.Min(k, s[i].Length);
                for (int j = 0; j < k; j++)
                {
                    if (s[i][j] != s[0][j])
                    {
                        k = j;
                        break;
                    }
                }
            }
            return s[0].Substring(0, k);
        }
    }
}