using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.Lightspeed
{
    public partial class Utility
    {
        private static MethodInfo _arrayResizeStaticMethod;
        private static object[] _resizeArrayParameters;
        
        public static object ResizeArrayGeneric(object array, int newSize)
        {
            var type = array.GetType();
            if (!type.IsArray)
                throw new ArgumentException($"array was of type {type.GetNiceName()}, expected ArrayType");
            
            var elemType = type.GetElementType();
            if (_arrayResizeStaticMethod == null)
                _arrayResizeStaticMethod = typeof(Array).GetMethod("Resize", BindingFlags.Static | BindingFlags.Public);
            
            var properResizeMethod = _arrayResizeStaticMethod.MakeGenericMethod(elemType);
            if (_resizeArrayParameters == null)
                _resizeArrayParameters = new object[2];
            _resizeArrayParameters[0] = array;
            _resizeArrayParameters[1] = newSize;
            
            properResizeMethod.Invoke(null, _resizeArrayParameters);
            array = _resizeArrayParameters[0];
            return array;
        }

        public static T[] JoinArrays<T>(T[] arr1, T[] arr2)
        {
            var resultArr = new T[arr1.Length + arr2.Length];
            arr1.CopyTo(resultArr, 0);
            arr2.CopyTo(resultArr, arr1.Length);
            return resultArr;
        }
        
        public static T[] JoinArrays<T>(T element1, T[] arr2)
        {
            var resultArr = new T[1 + arr2.Length];
            resultArr[0] = element1;
            arr2.CopyTo(resultArr, 1);
            return resultArr;
        }
        
        public static T[] JoinArrays<T>(T[] arr1, T element2)
        {
            var resultArr = new T[arr1.Length + 1];
            arr1.CopyTo(resultArr, 0);
            resultArr[arr1.Length] = element2;
            return resultArr;
        }
    }
}