using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhinox.Lightspeed
{
    public static class DictionaryExtensions
    {
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
        
        /// <summary>
        /// Where statement that returns a dictionary instead of the IEnumerable with a KeyValuePair
        /// </summary>
        public static Dictionary<TK, TV> DicWhere<TK, TV>(this IDictionary<TK, TV> dict,
            Func<KeyValuePair<TK, TV>, bool> predicate)
        {
            return dict.Where(predicate).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}