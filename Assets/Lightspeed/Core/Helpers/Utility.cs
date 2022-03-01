using System;
using System.Linq;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static string GetCommandLineArg(string name, string defaultValue = null)
        {
            var args = System.Environment.GetCommandLineArgs().ToList();
            int index = args.IndexOf(name);
            if (index >= 0 && index < args.Count - 1)
            {
                return args[index + 1];
            }
            return defaultValue;
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