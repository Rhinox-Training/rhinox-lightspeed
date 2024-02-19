using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rhinox.Lightspeed
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// [ExtensionMethod] Equals method but with any of the given
        /// </summary>
        public static bool EqualsOneOf<T>(this T first, params T[] others)
        {
            for (var i = 0; i < others.Length; i++)
            {
                if (first.Equals(others[i]))
                    return true;
            }

            return false;
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, T obj)
        {
            return list.Where(x => !x.Equals(obj));
        }

        public static T Minimize<T>(this IEnumerable<T> objects, Func<T, float> sortFunc)
        {
            float smallestVal = float.MaxValue;
            T minimal = default(T);
            foreach (var o in objects)
            {
                var metric = sortFunc(o);

                if (metric >= smallestVal)
                    continue;

                smallestVal = metric;
                minimal = o;
            }

            return minimal;
        }
        
        public static T Maximize<T>(this IEnumerable<T> objects, Func<T, float> sortFunc)
        {
            float largestValue = float.MinValue;
            T maximal = default(T);
            foreach (var o in objects)
            {
                var metric = sortFunc(o);

                if (metric <= largestValue)
                    continue;

                largestValue = metric;
                maximal = o;
            }

            return maximal;
        }

        public static float Minimize<T>(this IEnumerable<T> objects, T rootObj, Func<T, T, float> sortFunc)
        {
            float smallestVal = float.MaxValue;
            foreach (var o in objects)
            {
                var metric = sortFunc(rootObj, o);

                if (metric >= smallestVal)
                    continue;

                smallestVal = metric;
            }

            return smallestVal;
        }

        public static IEnumerable<T> SelectNonNull<TItem, T>(this IEnumerable<TItem> enumerable, Func<TItem, T> selector)
            where T : class
        {
            return enumerable.Select(selector).Where(x => x != null);
        }
        
        public static IEnumerable<T> SelectManyNonNull<TItem, T>(this IEnumerable<TItem> enumerable, Func<TItem, IEnumerable<T>> selector)
            where T : class
        {
            return enumerable.SelectMany(selector).Where(x => x != null);
        }

        /// <summary>
        /// Returns random objects from the list
        /// if specified number is larger than the capacity of the list, returns the list.
        /// </summary>
        public static List<T> GetRandomObjects<T>(this IEnumerable<T> list, int amountOfItems)
        {
            // get a list version
            var enumerable = list as IList<T> ?? list.ToList();

            // return original if wantedItems >= items
            if (enumerable.Count <= amountOfItems) return enumerable.ToList();

            // get some randoms
            List<T> returnList = new List<T>(amountOfItems);

            while (returnList.Count < amountOfItems)
            {
                returnList.Add(enumerable.Where(lay => !returnList.Contains(lay)).ToArray().GetRandomObject());
            }

            return returnList;
        }

        /// <summary>
        /// Returns a random object from the list
        /// </summary>
        public static T GetRandomObject<T>(this ICollection<T> list)
        {
            if (list.IsNullOrEmpty())
                return default(T);
            
            var i = UnityEngine.Random.Range(0, list.Count);
            return list.ElementAt(i);
        }

        public static T MaxOrDefault<T>(this ICollection<T> list, T defaultValue = default)
        {
            if (list == null || list.Count == 0)
                return defaultValue;
            return list.Max();
        }

        public static bool HasIndex<T>(this IEnumerable<T> list, int i)
        {
            return list != null && i >= 0 && i < list.Count();
        }

        public static bool HasIndex<T>(this IReadOnlyCollection<T> list, int i)
        {
            return list != null && i >= 0 && i < list.Count;
        }
        
        public static int RemoveAll<T>(this ICollection<T> set, Func<T, bool> condition)
        {
            return set.RemoveRange(set.Where(condition));
        }

        public static int RemoveRange<T>(this ICollection<T> set, IEnumerable<T> toRemove)
        {
            // the IEnumerable is potentially part of the Set; cast it to ensure it is not
            var l = toRemove.ToArray();
            int i = 0;
            foreach (var entry in l)
            {
                if (set.Remove(entry))
                    ++i;
            }

            return i;
        }

        public static bool RemoveFirst<T>(this ICollection<T> set, Func<T, bool> condition) where T : struct
        {
            if (set.Any(condition))
            {
                var entry = set.First(condition);
                return set.Remove(entry);
            }

            return false;
        }

        public static bool AddUnique<T>(this ICollection<T> list, T entry)
        {
            if (list.Contains(entry))
                return false;
            list.Add(entry);
            return true;
        }
        
        // Arrays will throw an error for this method
        public static void AddRange<T>(this ICollection<T> set, IEnumerable<T> collection)
        {
            // the IEnumerable is potentially part of the Set; cast it to ensure it is not
            // TODO: check VOLT and then remove this ToArray; This should not be done here
            var l = collection.ToArray();
            foreach (var entry in l)
                set.Add(entry);
        }
        
        public static bool ContainEqual<T>(this IEnumerable<T> set1, IEnumerable<T> set2)
        {
            if (set2 == null)
                return set1.IsNullOrEmpty();
            
            int entriesFound = 0;
            foreach (var entry in set2)
            {
                if (!set1.Contains(entry))
                    return false;

                ++entriesFound;
            }

            return entriesFound == set1.Count();
        }
        
        public static bool ContainEqualNonAlloc<T>(this ICollection<T> set1, IEnumerable<T> set2)
        {
            // IEnumerable version might allocate some comparerer
            if (set2 == null)
                return set1.IsNullOrEmpty();
            
            int entriesFound = 0;
            foreach (var entry in set2)
            {
                if (!set1.Contains(entry))
                    return false;

                ++entriesFound;
            }

            return entriesFound == set1.Count;
        }

        public static bool ContainsAny<T>(this ICollection<T> set1, ICollection<T> set2)
        {
            foreach (var entry in set1)
            {
                if (set2.Contains(entry))
                    return true;
            }

            return false;
        }
		
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

        public static ILookup<TKey, TValue> ToLookupMany<TKey, TGrouping, TGroupingValue, TValue>(this ICollection<TGrouping> values, Func<TGroupingValue, TKey> keySelector, Func<TGrouping, TValue> valueSelector)
            where TGrouping : ICollection<TGroupingValue>
        {
            return values
                .SelectMany(group => group.Select(value => (group, value)))
                .ToLookup(x => keySelector(x.value), x => valueSelector(x.group));
        }
        
        public static ILookup<TKey, TSource> ToLookupMany<TKey, TSource>(this ICollection<TSource> values, Func<TSource, ICollection<TKey>> keySelector)
        {
            return values
                .Select(source => (source, keys: keySelector(source)))
                .SelectMany(x => x.keys.Select(y => (x.source, key: y)))
                .ToLookup(x => x.key, x => x.source);
        }
        
        public static string StringJoin<T>(this IEnumerable<T> coll, string separator, Func<T, string> selector)
        {
            if (coll == null) return string.Empty;
            return string.Join(separator, coll.Select(selector));
        }
        
        public static string StringJoin<T, TIntermediate>(this IEnumerable<T> coll, string separator, Func<T, TIntermediate> selector)
        {
            if (coll == null) return string.Empty;
            var intermediateColl = coll.Select(selector);
            return StringJoin(intermediateColl, separator);
        }

        public static string StringJoin<T>(this IEnumerable<T> coll, string separator)
        {
            if (coll == null) return string.Empty;
            return string.Join(separator, coll.Select(x => x.ToString()));
        }
        
        public static bool IsNullOrEmpty<T>(this ICollection<T> coll)
        {
            return coll == null || coll.Count == 0;
        }
        
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> coll)
        {
            return coll == null || !coll.Any();
        }
        
        public static IEnumerable Enumerate(this IEnumerator enumerator)
        {
            yield return enumerator.Current;
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
        
        public static IEnumerable<T> Enumerate<T>(this IEnumerator<T> enumerator)
        {
            yield return enumerator.Current;
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
        
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
        {
            var pos = 0; 
            while (source.Skip(pos).Any())
            {
                yield return source.Skip(pos).Take(chunkSize);
                pos += chunkSize;
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
            return enumerable.OrderByDescending(selector);
        }
        
        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> enumerable, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
                return enumerable.ThenBy(selector);
            return enumerable.ThenByDescending(selector);
        }
    }
}