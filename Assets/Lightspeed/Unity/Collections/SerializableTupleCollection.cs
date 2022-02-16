using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rhinox.Lightspeed.Collections
{
    [Serializable]
    public struct SerializableTuple<TKey, TValue>
    {
        public TKey Item1;
        public TValue Item2;

        public SerializableTuple(TKey item1, TValue item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    public class SerializableTupleCollection
    {
    }

    [Serializable]
    public partial class SerializableTupleCollection<TKey, TValue> : SerializableTupleCollection,
        IEnumerable<SerializableTuple<TKey, TValue>>
    {
        [SerializeField]
        private List<SerializableTuple<TKey, TValue>> list = new List<SerializableTuple<TKey, TValue>>();

        public IEnumerator<SerializableTuple<TKey, TValue>> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // As disconnected Dictionary (for quick lookups)
        public Dictionary<TKey, TValue> ToDictionary() =>
            list.ToDictionary(element => element.Item1, element => element.Item2);

        public static SerializableTupleCollection<TKey, TValue> FromDictionary(IDictionary<TKey, TValue> dictionary)
        {
            var result = new SerializableTupleCollection<TKey, TValue>();
            result.list = dictionary.Select(kvp => new SerializableTuple<TKey, TValue>(kvp.Key, kvp.Value)).ToList();
            return result;
        }
    }
}