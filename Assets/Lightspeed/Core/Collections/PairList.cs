using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Lightspeed.Collections
{
    [Serializable]
    public struct SimplePair<T1, T2>
    {
        [HideLabel, HorizontalGroup] public T1 V1;
        [HideLabel, HorizontalGroup] public T2 V2;

        public SimplePair(T1 v1, T2 v2)
        {
            V1 = v1;
            V2 = v2;
        }
    }
    
    [Serializable]
    public class PairList<T1, T2> : CustomCollection<SimplePair<T1, T2>>
    {
        public IEnumerable<T1> Keys => _array.Select(x => x.V1);

        public IEnumerable<T2> Values => _array.Select(x => x.V2);

        public PairList()
        { }

        public void Add(T1 key, T2 value)
        {
            Add(new SimplePair<T1, T2>(key, value));
        }

        public void Add(T1 key, IEnumerable<T2> values)
        {
            foreach (var value in values)
                Add(new SimplePair<T1, T2>(key, value));
        }

        public IEnumerable<T2> GetValuesWithKey(T1 key)
        {
            return _array.Where(x => x.V1.Equals(key)).Select(x => x.V2);
        }

        public bool ContainsKey(T1 key)
        {
            for (var i = 0; i < Count; i++)
            {
                if (Equals(_array[i].V1, key)) return true;
            }

            return false;
        }

        public bool Get(T1 key, out T2 value)
        {
            for (var i = 0; i < Count; i++)
            {
                if (!Equals(_array[i].V1, key)) continue;

                value = _array[i].V2;
                return true;
            }

            value = default;
            return false;
        }

        public ILookup<T1, T2> ToLookup()
        {
            return _array.ToLookup(x => x.V1, x => x.V2);
        }

        public int FindIndex(Func<SimplePair<T1, T2>, bool> func)
        {
            if (func == null)
                return -1;
            
            for (int i = 0; i < _array.Length; ++i)
            {
                SimplePair<T1, T2> item = _array[i];
                if (func.Invoke(item))
                    return i;
            }
            return -1;
        }
    }
}