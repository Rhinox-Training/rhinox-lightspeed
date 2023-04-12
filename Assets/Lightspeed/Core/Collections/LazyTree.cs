using System;
using System.Collections.Generic;

namespace Rhinox.Lightspeed
{
    /// <summary>
    /// TreeNode that resolves its children as needed
    /// Support various methods to 'invalidate' the generated children (as the generator may have changed)
    /// </summary>
    public class LazyTree<T>
    {
        private TreeNode<T> _root;
        private Func<T, IEnumerable<T>> _childResolver;

        private HashSet<TreeNode<T>> _resolvedChildren; // keep track of what you resolved (dynamic resolving for traverse)
        private Stack<TreeNode<T>> _resolveStack; // just a cache to avoid allocation

        public LazyTree(T root, Func<T,  IEnumerable<T>> childResolver)
        {
            _root = new TreeNode<T>(root);
            _resolvedChildren = new HashSet<TreeNode<T>>();
            _resolveStack = new Stack<TreeNode<T>>();
            _childResolver = childResolver;
        }
        
        public void Traverse(Action<T> action)
        {
            // Resolve complete tree (will need it)
            Resolve();
            _root.Traverse(action);
        } 

        public void Traverse(Func<T, bool> action)
        { 
            // Resolve as required
            _root.Traverse(action, Resolve);
        }
        
        public void Traverse<TData>(TreeNode<T>.DataTraversalDelegate<TData> action, TData data = default)
            => _root.Traverse(action, data, Resolve);

        public void Refresh()
        {
            _resolvedChildren.Clear();
            _root.ClearChildren(true);
        }

        public void Resolve()
        {
            _resolveStack.Clear();
            _resolveStack.Push(_root);
            
            ResolveQueue(_resolveStack);
        }

        private void ResolveQueue(Stack<TreeNode<T>> stack)
        {
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                Resolve(node, addToResolveStack: true);
            }
        }

        private void Resolve(TreeNode<T> node) => Resolve(node, false);
        
        private void Resolve(TreeNode<T> node, bool addToResolveStack)
        {
            if (_resolvedChildren.Contains(node))
            {
                if (addToResolveStack)
                {
                    foreach (var child in node.Children)
                        _resolveStack.Push(child);
                }
                return;
            }
            
            IEnumerable<T> children = _childResolver?.Invoke(node.Value);
            if (children == null) return;

            // TODO check for existing children? clear them?
            foreach (var child in children)
            {
                var newNode = node.AddChild(child);
                if (addToResolveStack)
                    _resolveStack.Push(newNode);
            }

            _resolvedChildren.Add(node);
        }
    }
}