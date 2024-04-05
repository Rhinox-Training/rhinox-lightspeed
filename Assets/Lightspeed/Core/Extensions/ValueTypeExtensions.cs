using System;
using System.Collections.Generic;

namespace Rhinox.Lightspeed
{
    public static class ValueTypeExtensions
    {
        /// <summary>
        /// Usage:
        /// foreach (var (id, name) in nameByID) {  }
        /// </summary>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey k, out TValue v)
        {
            k = kvp.Key;
            v = kvp.Value;
        }
        
        public static bool IsSingleFlag<T>(this T e) where T : Enum
        {
            // => does not work if there are combinations defined
            // return Enum.IsDefined(typeof(T), e);
            
            // Checks if the value of e is a power of two. This assumes that a single flag values will be power of two.
            // Note: Will return TRUE when passing 0
            var eAsInt = Convert.ToInt32(e);
            return (eAsInt & (eAsInt - 1)) == 0;
        }
        
        public static bool IsMaxValue(this float val, float epsilon = .005f)
        {
            return Math.Abs(float.MaxValue - val) < epsilon;
        }
        
        public static bool IsMaxValue(this double val, float epsilon = .005f)
        {
            return Math.Abs(double.MaxValue - val) < epsilon;
        }
        
        public static float Map(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        
        public static double Map(this double value, double from1, double to1, double from2, double to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        
        public static bool IsBetween(this float number, float min, float max)
        {
            if (min > max)
                Utility.Swap(ref min, ref max);
            
            return number > min && number < max;
        }
        
        public static bool IsBetween(this double number, double min, double max)
        {
            if (min > max)
                Utility.Swap(ref min, ref max);
            
            return number > min && number < max;
        }
        
        public static bool LossyEquals(this float val1, float val2)
        {
            return Math.Abs(val1 - val2) < float.Epsilon;
        }
        
        public static bool LossyEquals(this double val1, double val2)
        {
            return Math.Abs(val1 - val2) < double.Epsilon;
        }
        
        public static bool LossyEquals(this float val1, float val2, float epsilon)
        {
            return Math.Abs(val1 - val2) < epsilon;
        }
        
        public static bool LossyEquals(this double val1, double val2, double epsilon)
        {
            return Math.Abs(val1 - val2) < epsilon;
        }
        
        public static bool IsBetweenIncl(this float f, float minValue, float maxValue)
        {
            return (f - minValue) > -float.Epsilon && (f - maxValue) < float.Epsilon;
        }
        
        public static bool IsBetweenIncl(this double f, double minValue, double maxValue)
        {
            return (f - minValue) > -double.Epsilon && (f - maxValue) < double.Epsilon;
        }

        public static bool IsDefault<T>(this T value)
        {
            if (typeof(T).IsClass)
                return value == null;
            return value.Equals(default(T));
        }
        
        public static int GetDigitCount(this int number)
        {
            return number.ToString().Length;
        }

        public static string MatchDigitCountWith(this int number, int other)
        {
            return number.ToString($"D{other.GetDigitCount()}");
        }
    }
}