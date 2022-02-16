using System;
using System.Collections;
using System.Collections.Generic;

namespace Rhinox.Lightspeed
{
    public static class EnumerableExtensions
    {
        // public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        // {
        //     var comparer = new ComparisonComparer<T>(comparison);
        //     ArrayList.Adapter(list).Sort(comparer);
// 
        // }
        
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
    }
}