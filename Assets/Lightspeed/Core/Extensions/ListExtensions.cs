using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhinox.Lightspeed
{
    public static class ListExtensions
    {
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
        
        public static void Move<T>(this IList<T> list, int fromIndex, int toIndex)
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

        public static void MoveUp<T>(this IList<T> list, T item)
        {
            var i = list.IndexOf(item);
            if (i < 0)
                throw new ArgumentException("Could not find item in list");
            list.Move(i, i-1);
        }
        
        public static void MoveDown<T>(this IList<T> list, T item)
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
        
        private static void CopyTo<T>(this IList<T> sourceList, IList<T> destinationList, int sourceIndex = 0, int destinationIndex = 0, int count = -1)
        {
            if (count == -1)
                count = sourceList.Count;
            
            for (int i = 0; i < count; i++)
                destinationList[destinationIndex + i] = sourceList[sourceIndex + i];
        }
        
        public static bool IsSorted<T>(this IList<T> src, bool descending = false) where T: IComparable
        {
            var comparer = Comparer<T>.Default;
            return IsSorted(src, comparer, descending);
        }
        
        public static bool IsSorted<T>(this IList<T> src, Comparison<T> comparison, bool descending = false)
        {
            var comparer = Comparer<T>.Create(comparison);
            return IsSorted(src, comparer, descending);
        }
        
        private static bool IsSorted<T>(this IList<T> src, Comparer<T> comparer, bool descending = false)
        {
            for (int i = 1; i < src.Count; i++)
            {
                // Returns 1 if y is greater; -1 if y is smaller; 0 if equal
                var comparison = comparer.Compare(src[i - 1], src[i]);
                if (comparison > 0 && !descending)
                    return false;
                if (comparison < 0 && descending)
                    return false;
            }
            return true;
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
        /// Returns the default value of type U if the key does not exist in the dictionary
        /// </summary>
        public static T GetOrDefault<T>(this IList<T> list, int index, T onMissing = default(T))
        {
            if (list == null || !list.HasIndex(index))
                return onMissing;
                
            return list[index];
        }
    }
}