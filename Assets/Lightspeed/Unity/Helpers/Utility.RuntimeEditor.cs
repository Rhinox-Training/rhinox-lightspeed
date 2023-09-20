using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
#endif

namespace Rhinox.Lightspeed
{
    /// <summary>
    /// Some methods that use the editor but can be used at runtime
    /// This is mostly to reduce the #IF UNITY_EDITOR in scripts
    /// </summary>
    public static partial class Utility
    {
        public static void Destroy(Object o, bool immediate = false)
        {
            if (o == null) return;
            if (immediate)
            {
                Object.DestroyImmediate(o);
                return;
            }
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Object.DestroyImmediate(o);
            else
                Object.Destroy(o);
#else
            Object.Destroy(o);
#endif
        }
        
        public static void DestroyObject(Component t)
        {
            if (t == null) return;
            Destroy(t.gameObject);
        }

        // This function only exist to ensure that the object is destroyed; mostly to prevent errors during refactors
        public static void DestroyObject(GameObject o, bool immediate = false)
            => Destroy(o, immediate);
    
        public static bool IsEditTime()
        {
#if UNITY_EDITOR
            return !EditorApplication.isPlayingOrWillChangePlaymode;
#else
            return false;
#endif
        }
    
        public static bool IsPrefab(GameObject obj)
        {
#if UNITY_EDITOR
            return PrefabUtility.IsPartOfAnyPrefab(obj);
#else
        return false;
#endif
        }
        
        public static bool IsEditingPrefab(GameObject obj)
        {
#if UNITY_EDITOR
            return PrefabStageUtility.GetPrefabStage(obj) != null;
#else
        return false;
#endif
        }

#if UNITY_EDITOR // these functions will still require #IF UNITY_EDITOR (parameters contain this restriction)
#if UNITY_2019_1_OR_NEWER
        public static void SubscribeToSceneGui(Action<SceneView> action)
        {
            SceneView.duringSceneGui -= action;
            SceneView.duringSceneGui += action;
        }
        
        public static void UnsubscribeFromSceneGui(Action<SceneView> action)
        {
            SceneView.duringSceneGui -= action;
        }
#else
        public static void SubscribeToSceneGui(SceneView.OnSceneFunc action)
        {
            SceneView.onSceneGUIDelegate -= action;
            SceneView.onSceneGUIDelegate += action;
        }
        
        public static void UnsubscribeToSceneGui(SceneView.OnSceneFunc action)
        {
            SceneView.onSceneGUIDelegate -= action;
        }
#endif
#endif
        
        public static GameObject CopyGameObject(GameObject src, HideFlags hideFlags, params Type[] componentTypes)
        {
#if UNITY_EDITOR
            var dict = new Dictionary<GameObject, GameObject>();
            var queue = new Queue<GameObject>();
            queue.Enqueue(src);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (cur == null)
                    continue;
                
                GameObject go = new GameObject(cur.name + "(Copy)");
                
                var curParent = cur.transform.parent;
                if (curParent != null && dict.ContainsKey(curParent.gameObject))
                {
                    go.transform.parent = dict[curParent.gameObject].transform;
                }
                
                go.transform.localPosition = cur.transform.localPosition;
                go.transform.localRotation = cur.transform.localRotation;
                go.transform.localScale = cur.transform.localScale;
                
                dict.Add(cur, go);

                foreach (Transform child in cur.transform)
                {
                    queue.Enqueue(child.gameObject);
                }
            }

            var componentCache = new List<Component>();
            foreach (var entry in dict)
            {
                entry.Key.GetComponents(componentCache);
                if (componentTypes != null && componentTypes.Length > 0)
                {
                    componentCache.RemoveAll(x => !componentTypes.Contains(x.GetType()));
                }

                foreach (var component in componentCache)
                {
                    var copyComponent = entry.Value.AddComponent(component.GetType());
                    EditorUtility.CopySerialized(component, copyComponent);
                }
            }

            var result = dict[src];
            result.hideFlags = hideFlags;
            return result;
#else
            throw new InvalidOperationException($"Copy of '{src.name}' failed, reason: cannot copy GameObjects using this method at runtime.");
#endif
        }

        public static GameObject CopyGameObject(GameObject src, params Type[] componentTypes)
        {
            return CopyGameObject(src, HideFlags.None, componentTypes);
        }
    }
}
