using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rhinox.Lightspeed
{
    public class PairList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>
    {
        public IEnumerable<TKey> Keys => this.Select(x => x.Key);

        public IEnumerable<TValue> Values => this.Select(x => x.Value);

        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public void Add(TKey key, IEnumerable<TValue> values)
        {
            foreach (var value in values)
                Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public IEnumerable<TValue> GetValuesWithKey(TKey key)
        {
            return this.Where(x => x.Key.Equals(key)).Select(x => x.Value);
        }

        public bool ContainsKey(TKey key)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (Equals(this[i].Key, key)) return true;
            }

            return false;
        }

        public bool Get(TKey key, out TValue value)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (!Equals(this[i].Key, key)) continue;

                value = this[i].Value;
                return true;
            }

            value = default;
            return false;
        }

        public ILookup<TKey, TValue> ToLookup()
        {
            return this.ToLookup(x => x.Key, x => x.Value);
        }
    }
}