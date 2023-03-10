using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Lightspeed.Collections
{
    [Serializable]
    [UnitySupportWarning(2020)] // Generic serialization with [SerializeReference] is only supported from 2020+
    public abstract class CustomCollection<T> : ICollection<T>
    {
        [SerializeField]
        protected T[] _array;
        
        [SerializeField]
        protected int _count;

        public CustomCollection()
        {
            _array = new T[4];
        }
        
        public T this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public IEnumerator<T> GetEnumerator() => new InternalEnumerator(_array, _count);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual void Add(T item)
        {
            if (_count >= _array.Length)
                ExtendBackingArray();
            _array[_count] = item;
            ++_count;
        }

        private void ExtendBackingArray()
        {
            // Extend array to accomodate the new item if needed
            var array = new T[_array.Length * 2];
            _array.CopyTo(array, 0);
            _array = array;
        }

        public void Clear()
        {
            for (var i = 0; i < _array.Length; i++)
            {
                OnItemRemoved(_array[i]);
                _array[i] = default;
            }

            _count = 0;
        }

        protected virtual void OnItemRemoved(T item) { }

        public bool Contains(T item) => _array.IndexOf(item) >= 0;

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_array.Length < _count)
                _count = _array.Length;
            Array.Copy(_array, arrayIndex, array, arrayIndex, _count - arrayIndex);
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);

            if (index < 0)
                return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            OnItemRemoved(_array[index]);

            // move everything one spot
            for (int i = _count-1; index > i; --i)
                _array[i-1] = _array[i];
            --_count;
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (_array[i].Equals(item))
                    return i;
            }
            
            return -1;
        }

        public int Count => _count;
        public virtual bool IsReadOnly => false;
        
        internal struct InternalEnumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private const int NOT_STARTED = -2;
            private const int FINISHED = -1;
            private readonly T[] array;
            private int idx;
            private int length;

            internal InternalEnumerator(T[] array, int length)
            {
                this.array = array;
                this.idx = NOT_STARTED;
                this.length = length;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (this.idx == NOT_STARTED)
                    this.idx = length;
                return this.idx != -1 && --this.idx != FINISHED;
            }

            public T Current
            {
                get
                {
                    if (this.idx == NOT_STARTED)
                        throw new InvalidOperationException("Enumeration has not started. Call MoveNext");
                    if (this.idx == FINISHED)
                        throw new InvalidOperationException("Enumeration already finished");
                    return this.array[length - 1 - this.idx];
                }
            }

            void IEnumerator.Reset() => this.idx = NOT_STARTED;

            object IEnumerator.Current => (object) this.Current;
        }
    }
}