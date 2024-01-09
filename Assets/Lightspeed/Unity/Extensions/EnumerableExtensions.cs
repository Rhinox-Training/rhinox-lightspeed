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
        
        public static void SortStable<T, TKey>(this IList<T> list, Func<T, TKey> selector, bool descending = false)
            where TKey : IComparable<TKey>
        {
            IList<T> orderedList;
            // We are clearing the original so we need to resolve it

            if (descending)
                orderedList = list.OrderByDescending(selector).ToList();
            else
                orderedList = list.OrderBy(selector).ToList();
            CopyTo(orderedList, list);
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

        public static ArraySegment<T> TakeSegment<T>(this T[] array, int offset)
            => TakeSegment(array, offset, array.Length - offset);

        public static ArraySegment<T> TakeSegment<T>(this T[] array, int offset, int count)
            => new ArraySegment<T>(array, offset, count);

        public static ArraySegment<T> TakeSegment<T>(this ArraySegment<T> segment, int offset)
            => TakeSegment(segment.Array, segment.Offset + offset, segment.Count - offset);
        
        public static ArraySegment<T> TakeSegment<T>(this ArraySegment<T> segment, int offset, int count)
            => TakeSegment(segment.Array, segment.Offset + offset, count);

        public static float StdDev(this IEnumerable<float> values)
        {
            //Compute the Average
            float avg = values.Average();

            //Perform the Sum of (value-avg)^2
            float sum = 0.0f;
            int count = 0;
            foreach (float val in values)
            {
                sum += ((val - avg) * (val - avg));
                ++count;
            }

            if (count == 0)
                return 0.0f;
            
            //Put it all together
            float stdDev = (float)Math.Sqrt(sum / count);
            return stdDev;
        }
        
        
        public static float StdDev<TKey>(this IEnumerable<TKey> values, Func<TKey, float> selector)
        {
            //Compute the Average
            float avg = values.Average(selector);

            //Perform the Sum of (value-avg)^2
            float sum = 0.0f;
            int count = 0;
            foreach (TKey val in values)
            {
                float floatVal = selector(val);
                sum += ((floatVal - avg) * (floatVal - avg));
                ++count;
            }

            if (count == 0)
                return 0.0f;
            
            //Put it all together
            float stdDev = (float)Math.Sqrt(sum / count);
            return stdDev;
        }
        
        public static double StdDev(this IEnumerable<double> values)
        {
            //Compute the Average
            double avg = values.Average();

            //Perform the Sum of (value-avg)^2
            double sum = 0.0;
            int count = 0;
            foreach (var val in values)
            {
                sum += ((val - avg) * (val - avg));
                ++count;
            }

            if (count == 0)
                return 0.0;
            
            //Put it all together
            var stdDev = Math.Sqrt(sum / count);
            return stdDev;
        }
        
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
                return enumerable.OrderBy(selector);
            else
                return enumerable.OrderByDescending(selector);
        }
        
        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> enumerable, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
                return enumerable.ThenBy(selector);
            else
                return enumerable.ThenByDescending(selector);
        }
    }
}