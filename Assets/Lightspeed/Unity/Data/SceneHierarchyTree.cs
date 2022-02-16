using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rhinox.Utilities
{
    public class SceneHierarchyTree
    {
        public struct PathData
        {
            public readonly string[] PathParts;
            public int CurrentIndex;
            public string Next => PathParts.GetAtIndex(CurrentIndex);

            public string ObjectName => PathParts.LastOrDefault();
            
            public bool IsRooted;

            public PathData(string path, int index = 0)
            {
                IsRooted = path.StartsWith("/");
                PathParts = path.Split('/').Where(x => !x.IsNullOrWhitespace()).ToArray();
                CurrentIndex = index;
            }
        }
        
        private static SceneHierarchyTree _cachedTree;
        private static bool _frozen; // Prevents tree from being refresh
        
        public List<LazyTree<GameObject>> RootTrees;

        private List<GameObject> _searchResults;
        private GameObject _exactMatch;
        private bool _enableFuzzySearch;

        public static GameObject Find(string path, bool fuzzy = false)
        {
            var tree = SceneHierarchyTree.Create();
            return tree.FindObject(path, fuzzy);
        }

        public static void Freeze(bool refresh = true)
        {
            BetterLog.Trace<UtilityLogger>($"Initiated freeze on SceneHierarchyTree...");

            if (refresh) RefreshState();
            _frozen = true;
        } 

        public static void UnFreeze()
        { 
            BetterLog.Trace<UtilityLogger>($"Released freeze on SceneHierarchyTree.");
            _frozen = false;
        }
        
        // Private constr; use the Create method to get one
        private SceneHierarchyTree()
        {
            RootTrees = new List<LazyTree<GameObject>>();
            _searchResults = new List<GameObject>();

#if UNITY_EDITOR
            // Load the active editor scene (only when not in build, otherwise the loop below will contain it)
            var currScene = SceneManager.GetActiveScene();
            if (!Utility.IsSceneInBuild(currScene))
                GetSceneObjectTrees(currScene, RootTrees);
#endif
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                GetSceneObjectTrees(s, RootTrees);
            }

            _cachedTree = this;
        }

        public GameObject FindObject(string path, bool fuzzy = false)
        {
            var pathData = new PathData(path);
            _searchResults.Clear();
            _exactMatch = null;
            _enableFuzzySearch = fuzzy;
            
            foreach (var tree in RootTrees)
                tree.Traverse(HandleFindTraversal, pathData);
            _enableFuzzySearch = false;

            var result = _exactMatch != null ? _exactMatch : _searchResults.FirstOrDefault();
            return result;
        }

        private bool HandleFindTraversal(GameObject go, ref PathData data)
        {
            // If we have enough results; just stop
            if (_enableFuzzySearch)
            {
                if (_exactMatch != null || _searchResults.Count >= RootTrees.Count) return false;

                if (go.name == data.ObjectName)
                    _searchResults.Add(go);
            }
            else
            {
                if (_searchResults.Any() || _exactMatch != null)
                    return false;
            }

            // check if it matches
            if (go.name == data.Next)
                data.CurrentIndex += 1;
            else // If rooted and it does not match: cancel search
                return !data.IsRooted;

            // Full match found
            if (data.CurrentIndex >= data.PathParts.Length)
            {
                _exactMatch = go;
                _searchResults.Insert(0, go); // Highest priority match, put first
                return false;
            }

            return true;
        }

        // In general you'll want a refreshed state tree but ...
        // in case you're going to do a lot of stuff utilizing this whilst changing very little; use false and refresh beforehand
        public static SceneHierarchyTree Create()
        {
            if (!_frozen || _cachedTree == null)
                return new SceneHierarchyTree();
            return _cachedTree;
        }

        // Use this method when doing as mentioned above
        public static void RefreshState()
        {
            _cachedTree = null;
        }
        
        private static void GetSceneObjectTrees(Scene s, ICollection<LazyTree<GameObject>> collection)
        {
            if (!s.isLoaded)
            {
                BetterLog.Error<UtilityLogger>($"SceneHierarchy find failed due to scene '{s.name}' not being ready.");
                return;
            }
            GameObject[] rootObjects = s.GetRootGameObjects();
            
            for (int i = 0; i < rootObjects.Length; i++)
                collection.Add(new LazyTree<GameObject>(rootObjects[i], GetChildren));
        }

        private static IEnumerable<GameObject> GetChildren(GameObject go)
        {
            foreach (Transform t in go.transform)
                yield return t.gameObject;
        }
    }
}