using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Rhinox.Lightspeed.Collections
{
    [Serializable]
    [HideReferenceObjectPicker, HideLabel]
    public class Toggleable<T>
    {
        public event Action<Toggleable<T>> ToggledChanged;
        
        [SerializeField]
        [HorizontalGroup(15), HideLabel]
        private bool _toggled;

        public bool Toggled
        {
            get => _toggled;
            set
            {
                _toggled = value;
                ToggledChanged?.Invoke(this);
            }
        }
        
        [HorizontalGroup, EnableIf(nameof(Toggled)), HideLabel]
        public T Item;

        public Toggleable(T item, bool toggled = true)
        {
            Item = item;
            Toggled = toggled;
        }
    
        public static implicit operator T(Toggleable<T> toggleable)
        {
            return toggleable == null ? default(T) : toggleable.Item;
        }
    }

    // NOTE: Requires Odin for proper rendering
    // TODO: Build custom drawer ?
    [Serializable]
    public class ToggleableList<T> : List<Toggleable<T>>
    {
        public delegate void ItemHandler(Toggleable<T> item);

        public event ItemHandler Toggled;
        
        public ToggleableList() { }
        
        public ToggleableList(ICollection<T> collection)
        {
            foreach (var item in collection)
                Add(item);
        }

        public virtual void Toggle(T item)
        {
            var toggleable = Find(item);
            toggleable.Toggled ^= true;
        }

        public Toggleable<T> Find(T item)
        {
            var i = IndexOf(item);
            return i < 0 ? null : this[i];
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (this[i] != null && this[i].Item.Equals(item))
                    return i;
            }
            
            return -1;
        }

        public virtual void Add(T item)
        {
            var toggleable = new Toggleable<T>(item);
            toggleable.ToggledChanged += OnToggled;
            base.Add(toggleable);
        }

        public virtual void Remove(T item)
        {
            var i = IndexOf(item);
            
            var toggleable = this[i];
            toggleable.ToggledChanged -= OnToggled;
            
            base.RemoveAt(i);
        }
        
        protected void OnToggled(Toggleable<T> item)
        {
            Toggled?.Invoke(item);
        }
    }

    public static class ToggleableExtensions
    {
        public static ToggleableList<T> AsToggleableList<T>(this ICollection<T> coll)
        {
            return new ToggleableList<T>(coll);
        }
    }
}