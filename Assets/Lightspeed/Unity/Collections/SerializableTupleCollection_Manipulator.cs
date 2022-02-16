#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public partial class SerializableTupleCollection<TKey, TValue> {

    public  IDictionary<TKey, TValue> Manipulator => manipulator ?? (manipulator = new CollectionManipulator(this));
    private IDictionary<TKey, TValue> manipulator;

    private class CollectionManipulator : IDictionary<TKey, TValue> {
        SerializableTupleCollection<TKey, TValue> collection;

        public CollectionManipulator (SerializableTupleCollection<TKey, TValue> collection) {
            this.collection = collection;
        }

#region IDictionary<TKey, TValue>
        public TValue this[TKey key] {
            get {
                var index = GetIndex(key);
                if (index == -1)
                    throw new KeyNotFoundException($"Key not found: {key}");
                return collection.list[index].Item2;

            }
            set {
                var index = GetIndex(key);
                if (index != -1) {
                    var p = collection.list[index];
                    p.Item2 = value;
                    collection.list[index] = p;
                }
                else
                    collection.list.Add( new SerializableTuple<TKey, TValue>( key, value ) );
            }
        }

        public ICollection<TKey>   Keys   => collection.list.Select( tuple => tuple.Item1 ).ToArray();
        public ICollection<TValue> Values => collection.list.Select( tuple => tuple.Item2 ).ToArray();

        public void Add(TKey key, TValue value) {
            if (GetIndex(key) != -1)
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            collection.list.Add( new SerializableTuple<TKey, TValue> ( key, value ) );
        }

        public bool ContainsKey(TKey key) => GetIndex(key) != -1;

        public bool Remove (TKey key) {
            var index = GetIndex(key);
            var removed = index != -1;
            if (removed)
                collection.list.RemoveAt(index);
            return removed;
        }

        public bool TryGetValue (TKey key, out TValue value) {
            var index = GetIndex(key);
            var gotValue = index != -1;
            value = gotValue ? collection.list[index].Item2 : default;
            return gotValue;
        }
#endregion


#region ICollection <KeyValuePair<TKey, TValue>>
        public int  Count      => collection.list.Count;
        public bool IsReadOnly => false;

        public void Add (KeyValuePair<TKey, TValue> item) => Add( item.Key, item.Value );

        public void Clear    ()                                => collection.list.Clear();
        public bool Contains (KeyValuePair<TKey, TValue> item) => GetIndex(item.Key) != -1;

        public void CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            var numKeys = collection.list.Count;
            if (array.Length - arrayIndex < numKeys)
                throw new ArgumentException();
            for (int i = 0; i < numKeys; i++, arrayIndex++) {
                var element = collection.list[i];
                array[arrayIndex] = new KeyValuePair<TKey, TValue>( element.Item1, element.Item2 );
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
#endregion


#region IEnumerable <KeyValuePair<TKey, TValue>>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator () {
            return collection.list.Select(ToKeyValuePair).GetEnumerator();

            KeyValuePair<TKey, TValue> ToKeyValuePair (SerializableTuple<TKey, TValue> sTuple) {
                return new KeyValuePair<TKey, TValue>( sTuple.Item1, sTuple.Item2 );
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
#endregion

        private static EqualityComparer<TKey> equalityComparer = EqualityComparer<TKey>.Default;

        private int GetIndex (TKey key) {
            var list        = collection.list;
            var numElements = list.Count;
            for (int i = 0; i < numElements; i++)
                if (equalityComparer.Equals(list[i].Item1, key))
                    return i;
            return -1;
        }
    }
}
#endif