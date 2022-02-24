using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rhinox.Lightspeed.Collections
{
    public class PriorityQueueEntry<TPrio, T>
    {
        public TPrio Priority { get; }
        public T Data { get; }
        
        public PriorityQueueEntry(TPrio priority, T data) 
        {
            Priority = priority;
            Data = data;
        }
    }

    public class PriorityQueue<TPrio, T> : IEnumerable<T>, IEnumerable where TPrio : IComparable
    {
        private LinkedList<PriorityQueueEntry<TPrio, T>> _list;

        public PriorityQueue()
        {
            _list = new LinkedList<PriorityQueueEntry<TPrio,T>>();
        }

        public PriorityQueue(IEnumerable<T> items, Func<T, TPrio> prioFetcher)
        {
            var orderedItems = items
                .Select(x => new PriorityQueueEntry<TPrio, T>(prioFetcher(x), x));
            
            _list = new LinkedList<PriorityQueueEntry<TPrio,T>>(orderedItems);
        }

        public int Count => _list.Count;

        public T this[int index] => _list.ElementAt(index).Data;

        public void Enqueue(T data, TPrio priority = default)
        {
            if (_list.Count == 0)
            {
                _list.AddFirst(new PriorityQueueEntry<TPrio,T>(priority, data));
                return;
            }
            
            LinkedListNode<PriorityQueueEntry<TPrio,T>> current = _list.First;
            while (current != null)
            {
                if (current.Value.Priority.CompareTo(priority) > 0)
                {
                    _list.AddBefore(current,new PriorityQueueEntry<TPrio,T>(priority, data));
                    return;
                }
                current = current.Next;
            }
            _list.AddLast(new PriorityQueueEntry<TPrio,T>(priority,data));
        }

        public T Dequeue()
        {
            var ret = _list.First.Value.Data;
            _list.RemoveFirst();
            return ret;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        
        [Serializable]
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            private int index;

            private PriorityQueue<TPrio, T> _queue;
            private T current;
            public T Current => current;

            public Enumerator(PriorityQueue<TPrio, T> q)
            {
                _queue = q;
                index = -1;
                current = default;
            }
            
            public bool MoveNext()
            {
                if (index >= _queue.Count)
                    return MoveNextRare();
                current = _queue[index];
                ++index;
                return true;
            }
            
            private bool MoveNextRare()
            {
                index = _queue.Count + 1;
                current = default;
                return false;
            }

            public void Reset()
            {
                index = -1;
                current = default;
            }
            
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _queue = null;
            }
        }
    }

    public static partial class Extensions
    {
        public static PriorityQueue<TPrio, T> ToPriorityQueue<TPrio, T>(this IEnumerable<T> list, Func<T, TPrio> prioFetcher)
            where TPrio : IComparable
        {
            return new PriorityQueue<TPrio, T>(list, prioFetcher);
        }
    }
}