using System;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace Rhinox.Lightspeed
{
    public static class EnumerableExtensions
    {
        public static void SortBy<T, TParam>(this IList<T> list, Func<T, TParam> sortFunc)
            where TParam: IComparable
        {
            var comparer = new Comparison<T>((x, y) => sortFunc(x).CompareTo(sortFunc(y)));
            list.Sort(comparer);
        }
        
        public static void SortByDescending<T, TParam>(this IList<T> list, Func<T, TParam> sortFunc)
            where TParam: IComparable
        {
            var comparer = new Comparison<T>((x, y) => sortFunc(y).CompareTo(sortFunc(x)));
            list.Sort(comparer);
        }
    }
}