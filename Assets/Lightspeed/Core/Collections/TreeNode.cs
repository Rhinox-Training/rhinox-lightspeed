using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhinox.Lightspeed
{
    public class TreeNode<T>
    {
        private readonly T _value;
        private readonly List<TreeNode<T>> _children = new List<TreeNode<T>>();

        public delegate bool DataTraversalDelegate<TData>(T o, ref TData data);

        public TreeNode(T value)
        {
            _value = value;
        }

        public TreeNode<T> this[int i] => _children[i];

        public TreeNode<T> Parent { get; private set; }

        public T Value => _value;

        public IReadOnlyCollection<TreeNode<T>> Children => _children.AsReadOnly();

        public bool HasNode(T value)
        {
            return GetNode(value) != null;
        }
        
        public TreeNode<T> GetNode(T value)
        {
            foreach (var child in _children)
                if (child.Value.Equals(value))
                    return child;
            return null;
        }

        public TreeNode<T> GetOrAddNode(T value)
        {
            foreach (var child in _children)
                if (child.Value.Equals(value))
                    return child;
            return AddChild(value);
        }

        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value) {Parent = this};
            _children.Add(node);
            return node;
        }

        public TreeNode<T>[] AddChildren(params T[] values)
        {
            var arr = new TreeNode<T>[values.Length];
            for (var i = 0; i < values.Length; i++)
                arr[i] = AddChild(values[i]);
            return arr;
        }

        public void ClearChildren(bool cascade = false)
        {
            if (cascade)
            {
                for (int i = 0; i < _children.Count; i++)
                    _children[i].ClearChildren(true);
            }
            
            _children.Clear();
        }

        public bool RemoveChild(TreeNode<T> node)
        {
            return _children.Remove(node);
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            for (var i = 0; i < _children.Count; i++)
                _children[i].Traverse(action);
        }
        
        /// <summary>
        /// Traverse the tree with a bool return indicating whether to continue this branch
        /// </summary>
        public void Traverse(Func<T, bool> action, Action<TreeNode<T>> callback)
        {
            callback?.Invoke(this);
            bool contTraverse = action(Value);
            if (!contTraverse) return;

            for (var i = 0; i < _children.Count; i++)
            {
                callback?.Invoke(_children[i]);
                _children[i].Traverse(action, callback);
            }
        }
        
        public void Traverse<TData>(DataTraversalDelegate<TData> action, TData data, Action<TreeNode<T>> callback)
        {
            callback?.Invoke(this);
            bool contTraverse = action(Value, ref data);
            if (!contTraverse) return;

            for (var i = 0; i < _children.Count; i++)
            {
                callback?.Invoke(_children[i]);
                _children[i].Traverse(action, data, callback);
            }
        }

        public List<TreeNode<T>> StartCustomTraversal(Action<List<TreeNode<T>>, List<TreeNode<T>>> action, TreeNode<T> startNode)
        {
            var nextList = new List<TreeNode<T>> { startNode };
            var resultList = new List<TreeNode<T>>();

            while (nextList.Count > 0)
            {
                action?.Invoke(nextList, resultList);
            }

            return resultList;
        }
        

        public IList<T> Flatten()
        {
            IList<T> list = new List<T>();
            Flatten(ref list);
            return list;
        }

        private void Flatten(ref IList<T> list)
        {
            for (int i = 0; i < _children.Count; ++i)
            {
                list.Add(Value);
                _children[i].Flatten(ref list);
            }
        }
    }
}