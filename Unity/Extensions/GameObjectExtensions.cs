using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.Lightspeed
{
    public static class GameObjectExtensions
    {
        public static T AddComponentWithInit<T>(this GameObject obj, System.Action<T> onInit) where T : Component
        {
            bool oldState = obj.activeSelf;
            obj.SetActive(false);
            T comp = obj.AddComponent<T>();
            onInit?.Invoke(comp);
            obj.SetActive(oldState);
            return comp;
        }

        public static GameObject[] GetAllChildren(this GameObject obj, bool includeInactive = false,
            bool includeSelf = true)
        {
            var results = obj.transform.GetAllChildren(includeInactive, includeSelf);
            var arr = new GameObject[results.Count];
            for (int index = 0; index < results.Count; index++)
                arr[index] = results[index].gameObject;
            
            return arr;
        }

        public static void GetAllChildren(this GameObject obj, IList<GameObject> gameObjects,
            bool includeInactive = false, bool includeSelf = true)
        {
            var results = obj.transform.GetAllChildren(includeInactive, includeSelf);
            foreach (var t in results)
                gameObjects.Add(t.gameObject);
        }

        public static void GetAllChildren(this GameObject obj, ISet<GameObject> gameObjects,
            bool includeInactive = false, bool includeSelf = true)
        {
            var results = obj.transform.GetAllChildren(includeInactive, includeSelf);
            foreach (var t in results)
                gameObjects.Add(t.gameObject);
        }

        public static void CopyObjectSettingsFrom(this GameObject to, GameObject from)
        {
#if UNITY_EDITOR
            GameObjectUtility.SetStaticEditorFlags(to, GameObjectUtility.GetStaticEditorFlags(from));
#endif
            to.layer = from.layer;
            to.tag = from.tag;
        }

        public static ICollection<GameObject> GetDirectChildren(this GameObject obj)
        {
            var list = new List<GameObject>();
            foreach (Transform child in obj.transform)
            {
                list.Add(child.gameObject);
            }

            return list;
        }

        public static T GetComponentInDirectChildren<T>(this GameObject go) where T : Component
        {
            var comp = go.GetComponentInChildren<T>();
            if (comp.transform.parent == go.transform)
                return comp;
            return null;
        }

        public static T[] GetComponentsInDirectChildren<T>(this GameObject go) where T : Component
        {
            return go.GetComponentsInChildren<T>()
                .Where(x => x.transform.parent == go.transform)
                .ToArray();
        }

        public static void DestroyAllChildren(this GameObject obj)
        {
            var children = obj.GetDirectChildren();
            foreach (var child in children)
                Utility.Destroy(child);
        }

        public static T GetComponentOnlyInChildren<T>(this GameObject gameObject) where T : Component
        {
            foreach (Transform child in gameObject.transform)
            {
                var comp = child.GetComponentInChildren<T>();
                if (comp != null)
                    return comp;
            }

            return null;
        }

        public static T[] GetComponentsOnlyInChildren<T>(this GameObject gameObject) where T : Component
        {
            var childComponents = new List<T>();
            foreach (Transform child in gameObject.transform)
            {
                var comp = child.GetComponentsInChildren<T>();
                if (comp == null) continue;

                childComponents.AddRange(comp);
            }

            return childComponents.ToArray();
        }

        public static GameObject AddChild(this GameObject go, string name = null)
        {
            var childGo = new GameObject();
            if (name != null)
                childGo.name = name;
            childGo.transform.SetParent(go.transform, false);
            return childGo;
        }

        public static GameObject AddChild(this GameObject go, string name, params Type[] componentTypes)
        {
            var childGo = new GameObject(name ?? "New GameObject", componentTypes);
            childGo.transform.SetParent(go.transform, false);
            return childGo;
        }

        public static T AddChildWithComponent<T>(this GameObject go, string name = null) where T : Component
        {
            var childGo = AddChild(go, name, typeof(T));
            return childGo.GetComponent<T>();
        }

        public static T AddComponent<T>(this GameObject go, T compToCopy)
            where T : Component
        {
            var newComp = go.AddComponent<T>();
            newComp.CopyDataFrom(compToCopy);
            return newComp;
        }

        public static bool TryGetComponentInParent<T>(this GameObject go, out T result)
        {
            result = go.GetComponentInParent<T>();
            return result != null;
        }

        public static bool TryGetComponentsInParent<T>(this GameObject go, out T[] results,
            bool includeInactive = false)
        {
            results = go.GetComponentsInParent<T>(includeInactive);
            return results.IsNullOrEmpty();
        }

        public static bool TryGetComponentInChildren<T>(this GameObject go, out T result)
        {
            result = go.GetComponentInChildren<T>();
            return result != null;
        }

        public static bool TryGetComponentsInChildren<T>(this GameObject go, out T[] results,
            bool includeInactive = false)
        {
            results = go.GetComponentsInChildren<T>(includeInactive);
            return results.IsNullOrEmpty();
        }

        /// <summary>
        /// Does the action for each child of the given parent. The action has 1 param [GameObject] which is the child.
        /// </summary>
        public static void ForeachChild(this GameObject parent, Action<GameObject> execute)
        {
            parent.transform.ForeachChild(execute);
        }

        /// <summary>
        /// Does the action for each child of the given parent. The action has 2 param [GameObject] which is the child and an [int] which is the index of the child.
        /// </summary>
        public static void ForAllChilderen(this GameObject parent, Action<GameObject, int> execute)
        {
            parent.transform.ForAllChilderen(execute);
        }

        public static string GetFullName(this GameObject obj)
        {
            if (obj == null)
                return string.Empty;

            Transform t = obj.transform;
            StringBuilder sb = new StringBuilder();
            while (t != null)
            {
                if (sb.Length == 0)
                {
                    sb.Append(t.name);
                }
                else
                {
                    sb.Insert(0, "/");
                    sb.Insert(0, t.name);
                }
                t = t.parent;
            }

            return sb.ToString();
        }

        public static bool IsEmpty(this GameObject go)
        {
            if (go == null)
                return true;

            var children = go.GetDirectChildren();
            if (children.Count > 0)
                return false;

            var comps = go.GetComponents(typeof(Component));
            return comps.All(x => x is Transform);
        }
    }
}