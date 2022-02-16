using System;
using System.Text.RegularExpressions;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static string NumToAlphabet(int value, bool caps = false)
        {
            string result = string.Empty;
            while (--value >= 0)
            {
                result = (char)(caps ? 'A' : 'a' + value % 26 ) + result;
                value /= 26;
            }
            return result;
        }
        
        public static int AlphabetToNum(string str)
        {
            int n = 0;
            for (int i = 0; i < str.Length; ++i)
            {
                // TODO isLetter allows for some foreign letters to pass; not sure how to handle that
                if (!Char.IsLetter(str[i]))
                    continue;
                
                var value = GetLetterValue(str[i]);
                n += IntPow(26,  str.Length - i - 1) * value;
            }
            return n;
        }
        
        // Will fail for negative Pow values
        private static int IntPow(int x, int pow)
        {
            int ret = 1;
            while ( pow != 0 )
            {
                if ( (pow & 1) == 1 )
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }
        
        // TODO check if an actual letter? [a-zA-Z]
        public static int GetLetterValue(char c) => c % 32;

        private static Regex _alphabetNumberingRegex;

        public static Group[] FindAlphabetNumbering(string str)
        {
            if (_alphabetNumberingRegex == null)
                _alphabetNumberingRegex = new Regex(@"[\d]([a-z]+|[A-Z]+)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var matches = _alphabetNumberingRegex.Matches(str);
            var arr = new Group[matches.Count];
            for (int i = 0; i < matches.Count; ++i)
            {
                // Groups array's 0th element is the entire match; then followed by the regex groups
                arr[i] = matches[i].Groups[1];
            }

            return arr;
        }
    }
}