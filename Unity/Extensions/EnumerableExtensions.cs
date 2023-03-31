using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rhinox.Lightspeed
{
    public static class EnumerableExtensions
    {
        private static void CopyTo<T>(this IList<T> sourceList, IList<T> destinationList, int sourceIndex = 0, int destinationIndex = 0, int count = -1)
        {
            if (count == -1)
                count = sourceList.Count;
            
            for (int i = 0; i < count; i++)
                destinationList[destinationIndex + i] = sourceList[sourceIndex + i];
        }
        
        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list is List<T>)
            {
                ((List<T>)list).Sort(comparison);
            }
            else
            {
                List<T> copy = new List<T>(list);
                copy.Sort(comparison);
                copy.CopyTo(list);
            }
        }
        
        public static void SortBy<T, TParam>(this IList<T> list, Func<T, TParam> sortFunc)
            where TParam: IComparable
        {
            var comparison = new Comparison<T>((x, y) => sortFunc(x).CompareTo(sortFunc(y)));
            
            list.Sort(comparison);

        }
        
        public static void SortByDescending<T, TParam>(this IList<T> list, Func<T, TParam> sortFunc)
            where TParam: IComparable
        {
            var comparison = new Comparison<T>((x, y) => sortFunc(y).CompareTo(sortFunc(x)));
            list.Sort(comparison);
        }
        
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            var pos = 0; 
            while (source.Skip(pos).Any())
            {
                yield return source.Skip(pos).Take(chunksize);
                pos += chunksize;
            }
        }
        
        public static IEnumerable<T> Flatten<T, TKey, TValue>(this IDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, IEnumerable<T>> flattenFunc)
        {
            foreach (var pair in source)
            {
                IEnumerable<T> resultCollection = flattenFunc.Invoke(pair);
                
                foreach (var result in resultCollection)
                    yield return result;
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source, Func<IEnumerable<T>, IEnumerable<T>> flattenFunc = null)
        {
            foreach (var collection in source)
            {
                IEnumerable<T> resultCollection = flattenFunc != null ? flattenFunc.Invoke(collection) : collection;
                
                foreach (var result in resultCollection)
                    yield return result;
            }
        }

    }
}