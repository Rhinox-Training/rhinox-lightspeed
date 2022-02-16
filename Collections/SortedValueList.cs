using System.Collections;
using System.Collections.Generic;

namespace Rhinox.Lightspeed
{
    public class SortedIntList : IEnumerable<int>
    {
        private List<int> _inner = new List<int>();

        public void Add(int item)
        {
            int insertionIndex = 0;
            for (int i = 0; i < _inner.Count; ++i)
            {
                if (item > _inner[i])
                {
                    insertionIndex = i;
                    break;
                }
            }

            _inner.Insert(insertionIndex, item);
        }

        public bool Contains(int item)
        {
            return _inner.Contains(item);
        }

        public IEnumerator<int> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}