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
    [HideReferenceObjectPicker, HideLabel]
    [Serializable]
    public class Toggleable<T>
    {
        public event Action<Toggleable<T>> ToggledChanged;
        
        [SerializeField, HorizontalGroup(15), HideLabel]
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

        public Toggleable()
        {
            _toggled = true;
        }
        
        public Toggleable(T item, bool toggled = true)
        {
            Item = item;
            _toggled = toggled;
            ToggledChanged = null;
        }
    
        public static implicit operator T(Toggleable<T> toggleable)
        {
            return toggleable.Item;
        }
    }

    [Serializable, HideReferenceObjectPicker]
    public class ToggleableList<T> : CustomCollection<Toggleable<T>>
    {
        public delegate void ItemHandler(Toggleable<T> item);
        
        public event ItemHandler Toggled;

        public ToggleableList()
        { }

        public ToggleableList(ICollection<T> collection)
        {
            foreach (var item in collection)
                Add(item);
        }
        
        public ToggleableList(ICollection<Toggleable<T>> collection)
        {
            foreach (var item in collection)
                Add(item.Item, item.Toggled);
        }

        public virtual void Toggle(T item)
        {
            var toggleable = Find(item);
            toggleable.Toggled ^= true;
            // Toggled Is hooked onto Toggleable's event; Do not call it here
        }

        public Toggleable<T> Find(T item)
        {
            var i = IndexOf(item);
            return i < 0 ? default : _array[i];
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (_array[i].Item.Equals(item))
                    return i;
            }
            
            return -1;
        }

        public void Add(T item, bool toggled = true)
        {
            var toggleable = new Toggleable<T>(item, toggled);
            base.Add(toggleable);
        }

        public override void Add(Toggleable<T> t)
        {
            if (t == null)
                t = new Toggleable<T>(default);
            t.ToggledChanged += OnToggled;
            
            base.Add(t);
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);

            if (index < 0)
                return false;

            RemoveAt(index);
            return true;
        }

        protected override void OnItemRemoved(Toggleable<T> item)
        {
            if (item != null)
                item.ToggledChanged -= OnToggled;
        }

        private void OnToggled(Toggleable<T> item)
        {
            Toggled?.Invoke(item);
        }

        public bool Contains(T item) => IndexOf(item) >= 0;
    }

    public static class ToggleableExtensions
    {
        public static ToggleableList<T> AsToggleableList<T>(this ICollection<T> coll)
        {
            return new ToggleableList<T>(coll);
        }
    }
}