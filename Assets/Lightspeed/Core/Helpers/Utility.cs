using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
        
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
        
        public static string FormatSizeBinary(long num, int toBase)
        {
            return Convert.ToString(num, toBase);
        }

        public static int GetHashCode(params object[] args)
        {
            if (args.Length == 1) return args[0].GetHashCode();
            int hashCode = 0x00FF;
            
            for (var i = 0; i < args.Length; i++)
                hashCode ^= args[i].GetHashCode();

            return hashCode;
        }
        
        public static string GetDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
        
        public static string GetLongestCommonPrefix(params string[] s)
        {
            if (s.IsNullOrEmpty())
                return string.Empty;
            
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