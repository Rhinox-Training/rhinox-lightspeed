using System;
using System.Reflection;

namespace Rhinox.Lightspeed
{
    public static class ArrayExtensions
    {
        private static MethodInfo _removeAtMethod;
        public static Array RemoveAtGeneric(this Array arr, int index)
        {
            var type = arr.GetType();
            var elemType = type.GetElementType();
            if (_removeAtMethod == null)
                _removeAtMethod = typeof(CollectionExtensions).GetMethod(nameof(RemoveAt), BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var properRemoveMethod = _removeAtMethod.MakeGenericMethod(elemType);
            var parameters = new object[] { arr, index };
            var array = (Array) properRemoveMethod.Invoke(null, parameters);
            return array;
        }
        
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if( index > 0 )
                Array.Copy(source, 0, dest, 0, index);

            if( index < source.Length - 1 )
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }
        
        public static bool IsRectangular<T>(this T[][] arr)
        {
            int size = -1;
            for (int i = 0; i < arr.Length; ++i)
            {
                if (size == -1)
                {
                    size = arr[i].Length;
                    continue;
                }

                if (size != arr[i].Length)
                    return false;
            }

            return true;
        }
        
        public static ArraySegment<T> TakeSegment<T>(this T[] array, int offset)
            => TakeSegment(array, offset, array.Length - offset);

        public static ArraySegment<T> TakeSegment<T>(this T[] array, int offset, int count)
            => new ArraySegment<T>(array, offset, count);

        public static ArraySegment<T> TakeSegment<T>(this ArraySegment<T> segment, int offset)
            => TakeSegment(segment.Array, segment.Offset + offset, segment.Count - offset);
        
        public static ArraySegment<T> TakeSegment<T>(this ArraySegment<T> segment, int offset, int count)
            => TakeSegment(segment.Array, segment.Offset + offset, count);
    }
}