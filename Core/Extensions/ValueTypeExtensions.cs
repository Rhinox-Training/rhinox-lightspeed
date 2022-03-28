using System;

namespace Rhinox.Lightspeed
{
    public static class ValueTypeExtensions
    {
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
            return f > (minValue - float.Epsilon) && f < (maxValue + float.Epsilon);
        }

        public static bool IsDefault<T>(this T value)
        {
            return value.Equals(default(T));
        }
    }
}