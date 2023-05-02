using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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
        
        /// <summary>
        /// Swaps 2 elements in a List
        /// </summary>
        public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }

        public static IList<T> Swap<T>(this IList<T> list, T objA, T objB)
        {
            var indexA = list.IndexOf(objA);
            var indexB = list.IndexOf(objB);

            T tmp = objA;
            list[indexA] = list[indexB];
            list[indexB] = tmp;

            return list;
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
        
        public static bool IsSorted<T>(this List<T> src) where T: IComparable
        {
            var comparer = Comparer<T>.Default;

            for (int i = 1; i < src.Count; i++)
            {
                if (comparer.Compare(src[i-1], src[i]) == 1)
                    return false;
            }
            return true;
        }
        
        public static bool IsSorted<T>(this List<T> src, Comparison<T> comparison)
        {
            var comparer = Comparer<T>.Create(comparison);

            for (int i = 1; i < src.Count; i++)
            {
                if (comparer.Compare(src[i-1], src[i]) == 1)
                    return false;
            }
            return true;
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

        /*public static T GetAtIndex<T>(this ICollection<T> list, int i, T defaultValue = default(T)) {
            return list.HasIndex(i) ? list.ElementAt(i) : defaultValue;
        }*/

        public static T GetAtIndex<T>(this IReadOnlyList<T> list, int i, T defaultValue = default(T))
        {
            return list.HasIndex(i) ? list[i] : defaultValue;
        }

        public static int FindIndex<T>(this IReadOnlyList<T> list, Func<T, bool> predicate)
        {
            for (int i = 0; i < list.Count; ++i)
                if (predicate(list[i]))
                    return i;

            return -1;
        }
        
        public static int IndexOf<T>(this IReadOnlyList<T> list, T elementToFind)
        {
            for (var i = 0; i < list.Count; ++i)
            {
                if (Equals(list[i], elementToFind))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Where statement that returns a dictionary instead of the IEnumerable with a KeyValuePair
        /// </summary>
        public static Dictionary<TK, TV> DicWhere<TK, TV>(this IDictionary<TK, TV> dict,
            Func<KeyValuePair<TK, TV>, bool> predicate)
        {
            return dict.Where(predicate).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// Usage:
        /// foreach (var (id, name) in nameByID) {  }
        /// </summary>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey k, out TValue v)
        {
            k = kvp.Key;
            v = kvp.Value;
        }
        
        /// <summary>
        /// Returns the default value of type U if the key does not exist in the dictionary
        /// </summary>
        public static T GetOrDefault<T>(this IList<T> list, int index, T onMissing = default(T))
        {
            if (list == null || !list.HasIndex(index))
                return onMissing;
                
            return list[index];
        }

        /// <summary>
        /// Returns the default value of type U if the key does not exist in the dictionary
        /// </summary>
        public static TV GetOrDefault<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV onMissing = default(TV))
        {
            if (key == null)
                return onMissing;
            TV value;
            return dict.TryGetValue(key, out value) ? value : onMissing;
        }

        public static TV GetOrDefault<TK, TV>(this IDictionary<TK, object> dict, TK key, TV onMissing = default(TV))
        {
            object o = dict.GetOrDefault(key);
            if (o == null)
                return onMissing;
            return (TV) Convert.ChangeType(o, typeof(TV));
        }
        
        public static bool ContainsNonDefault<TK, TV>(this IDictionary<TK, TV> dict, TK key, out TV value)
        {
            if (dict.IsNullOrEmpty())
            {
                value = default;
                return false;
            }
            
            if (dict.TryGetValue(key, out value))
                return !value.IsDefault();

            return false;
        }
        
        public static bool ContainsNonDefault<TK, TV>(this IDictionary<TK, TV> dict, TK key)
        {
            if (dict.IsNullOrEmpty()) return false;
            
            if (dict.TryGetValue(key, out TV value))
                return !value.IsDefault();

            return false;
        }

        /// <summary>
        /// Removes all elements where the given condition is true.
        /// </summary>
        public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, bool> condition)
        {
            var keys = dict.Keys.ToList();

            foreach (var key in keys)
            {
                if (dict.ContainsKey(key) && condition(key, dict[key]))
                    dict.Remove(key);
            }
        }
        
        public static void RemoveAll<T>(this ICollection<T> set, Func<T, bool> condition)
        {
            set.RemoveRange(set.Where(condition));
        }

        public static void RemoveRange<T>(this ICollection<T> set, IEnumerable<T> toRemove)
        {
            // the IEnumerable is potentially part of the Set; cast it to ensure it is not
            var l = toRemove.ToArray();
            foreach (var entry in l)
                set.Remove(entry);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                return set1 == null || set1.Count == 0;
            
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
        
        public static void Move<T>(this List<T> list, int fromIndex, int toIndex)
        {
            if (0 > fromIndex || fromIndex >= list.Count)
                throw new ArgumentException("From index is invalid");
            if (0 > toIndex || toIndex >= list.Count)
                throw new ArgumentException("To index is invalid");

            if (fromIndex == toIndex) return;

            var i = 0;
            T tmp = list[fromIndex];
            // Move element down and shift other elements up
            if (fromIndex < toIndex)
            {
                for (i = fromIndex; i < toIndex; i++)
                    list[i] = list[i + 1];
            }
            // Move element up and shift other elements down
            else
            {
                for (i = fromIndex; i > toIndex; i--)
                {
                    list[i] = list[i - 1];
                }
            }
            
            // put element from position 1 to destination
            list[toIndex] = tmp;
        }

        public static void MoveUp<T>(this List<T> list, T item)
        {
            var i = list.IndexOf(item);
            if (i < 0)
                throw new ArgumentException("Could not find item in list");
            list.Move(i, i-1);
        }
        
        public static void MoveDown<T>(this List<T> list, T item)
        {
            var i = list.IndexOf(item);
            if (i < 0)
                throw new ArgumentException("Could not find item in list");
            list.Move(i, i+1);
        }

        public static bool Contains<ItemT, T>(this IList<ItemT> list, T value)
            where ItemT : IEquatable<T>
        {
            if (list.IsNullOrEmpty()) return false;
            
            for (var i = 0; i < list.Count; i++)
                if (list[i].Equals(value))
                    return true;

            return false;
        }
        
        public static string StringJoin<T>(this IEnumerable<T> coll, string separator, Func<T, string> selector)
        {
            if (coll.IsNullOrEmpty()) return string.Empty;
            // TODO optimize

            return string.Join(separator, coll.Select(selector));
        }
        
        public static string StringJoin<T, TIntermediate>(this IEnumerable<T> coll, string separator, Func<T, TIntermediate> selector)
        {
            if (coll.IsNullOrEmpty()) return string.Empty;
            // TODO optimize

            var intermediateColl = coll.Select(selector);
            return StringJoin(intermediateColl, separator);
        }

        public static string StringJoin<T>(this IEnumerable<T> coll, string separator)
        {
            if (coll.IsNullOrEmpty()) return string.Empty;

            // TODO optimize
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
    }
}