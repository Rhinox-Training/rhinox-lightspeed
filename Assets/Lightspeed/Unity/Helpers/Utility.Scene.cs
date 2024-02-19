using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        /// Use this method to get all loaded scene objects of some type, including inactive objects. 
        /// <para>This is an alternative to Resources.FindObjectsOfTypeAll (returns project assets, including prefabs), and GameObject.FindObjectsOfTypeAll (deprecated).</para>
        public static void FindSceneObjectsOfTypeAll<T>(IList<T> results)
        {
            if (results == null)
                throw new ArgumentException("Given list must be initialized.");

#if UNITY_EDITOR
            // Load the active editor scene (only when not in build, otherwise the below will contain it)
            var currScene = SceneManager.GetActiveScene();
            if (!IsSceneInBuild(currScene))
                FindObjectsInScene(currScene, ref results);
#endif
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
#if UNITY_EDITOR
                if (object.Equals(s, currScene))
                    continue;
#endif
                FindObjectsInScene(s, ref results);
            }
        }
        
        public static void FindSceneObjectsOfTypeAll<T>(ISet<T> results)
        {
            if (results == null)
                throw new ArgumentException("Given list must be initialized.");

#if UNITY_EDITOR
            // Load the active editor scene (only when not in build, otherwise the below will contain it)
            var currScene = SceneManager.GetActiveScene();
            if (!IsSceneInBuild(currScene))
                FindObjectsInScene(currScene, ref results);
#endif
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                FindObjectsInScene(s, ref results);
            }
        }

        private static void FindObjectsInScene<T>(Scene s, ref IList<T> results)
        {
            if (!s.isLoaded) return;
            
            GameObject[] rootObjects = s.GetRootGameObjects();
            for (int j = 0; j < rootObjects.Length; j++)
            {
                var go = rootObjects[j];
                if (results is IList<GameObject> typedResults)
                    go.GetAllChildren(typedResults, true);
                else
                {
                    foreach (var child in go.GetComponentsInChildren<T>(true))
                        results.Add(child);
                }
            }
        }
        
        private static void FindObjectsInScene<T>(Scene s, ref ISet<T> results)
        {
            if (!s.isLoaded) return;
            
            GameObject[] rootObjects = s.GetRootGameObjects();
            for (int j = 0; j < rootObjects.Length; j++)
            {
                var go = rootObjects[j];
                if (results is ISet<GameObject> typedResults)
                    go.GetAllChildren(typedResults, true);
                else
                {
                    foreach (var child in go.GetComponentsInChildren<T>(true))
                        results.Add(child);
                }
            }
        }

        /// <summary>
        /// Does NOT find inactive objects
        /// </summary>
        public static GameObject FindInScene(string path, bool loosely = true)
        {
            var obj = GameObject.Find(path);

            if (obj != null || !loosely) return obj;
            
            // Find substitute
            if (FindFirstPartInScene(path, out string partialFirst, out GameObject baseObj))
            {
                // Above finds the 'beginning part'; aka the root, now find a child with the remainder
                // TODO: does this even find anything? Above should find it if this works
                var remainder = path.Substring(partialFirst.Length);
                var foundChild = baseObj.transform.Find(remainder);
                if (foundChild) return foundChild.gameObject;
            }

            if (FindLastPartInScene(path, out string partialLast, out GameObject childObj))
            {
                // TODO: limit how freely this returns?
                return childObj;
            }
            
            return obj;
        }

        private static bool FindFirstPartInScene(string path, out string foundPath, out GameObject obj)
        {
            string Strip(string p) => Path.GetDirectoryName(p);

            return FindInScene(path, Strip, out foundPath, out obj);
        }

        private static bool FindLastPartInScene(string path, out string foundPath, out GameObject obj)
        {
            string Strip(string p) => p.Substring(p.IndexOf('/')+1);
            
            return FindInScene(path, Strip, out foundPath, out obj);
        }

        /// <summary>
        /// Does NOT find inactive objects
        /// </summary>
        private static bool FindInScene(string path, Func<string, string> strip, out string foundPath, out GameObject obj)
        {
            obj = null;
            foundPath = strip(path);
            while (foundPath != null && foundPath.Contains("/"))
            {
                obj = GameObject.Find(foundPath);
                if (obj != null) break;
                foundPath = strip(foundPath);
            }

            return obj != null;
        }
        
        public static bool IsSceneInBuild(Scene s)
        {
            return s.buildIndex >= 0;
        }
    }
}