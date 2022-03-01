using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Reflection;
using UnityEngine.SceneManagement;
using Component = UnityEngine.Component;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        public static GameObject Create(string name, GameObject parent, params Type[] components)
        {
            return Create(name, parent.transform, components);
        }
        
        public static GameObject Create(string name, Transform parent, params Type[] components)
        {
            var go = new GameObject(name, components);
            go.transform.parent = parent.transform;
            go.transform.Reset();
            return go;
        }

        public static MeshRenderer Create(string name, GameObject parent, Mesh mesh, params Material[] materials)
            => Create(name, parent.transform, mesh, materials);
        
        public static MeshRenderer Create(string name, Transform parent, Mesh mesh, params Material[] materials)
        {
            var obj = Create(name, parent.transform);
            var filter = obj.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            var renderer = obj.AddComponent<MeshRenderer>();
            renderer.materials = materials;
            return renderer;
        }

        public static Gradient MakeGradient(Color start, Color end, params (float alpha, float time)[] alphaKeys)
        {
            var gradient = new Gradient();
            gradient.colorKeys = new[]
            {
                new GradientColorKey(start, 0),
                new GradientColorKey(end, 1)
            };

            if (alphaKeys.Length == 0) alphaKeys = new (float alpha, float time)[] { (0, 0), (1, 1) };
            gradient.alphaKeys = alphaKeys.Select(x => new GradientAlphaKey(x.alpha, x.time)).ToArray();
            return gradient;
        }
        
        

        public static void SetLayerRecursively<T>(this T obj, int newLayer) where T : Component
        {
            obj.gameObject.SetLayerRecursively(newLayer);
        }

        public static void SetLayerRecursively(this GameObject obj, int newLayer)
        {
            if (null == obj)
                return;

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (child == null)
                    continue;

                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public static void SetTagRecursively<T>(this T obj, string newTag) where T : Component
        {
            obj.gameObject.SetTagRecursively(newTag);
        }

        public static void SetTagRecursively(this GameObject obj, string newTag)
        {
            if (null == obj)
                return;
            
            obj.tag = newTag;

            foreach (Transform child in obj.transform)
            {
                if (child == null)
                    continue;

                SetTagRecursively(child.gameObject, newTag);
            }
        }

        /// <summary>
        /// Does a raycast from where you clicked, infinite length
        /// </summary>
        public static bool ClickRaycast(out RaycastHit hitInfo, LayerMask mask)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out hitInfo, Mathf.Infinity, mask);
        }

        /// <summary>
        /// Does a raycast from where you clicked, infinite length, any layer
        /// </summary>
        public static bool ClickRaycast(out RaycastHit hitInfo)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out hitInfo);
        }

        public static string FormatSizeBinary(long num, int toBase)
        {
            return Convert.ToString(num, toBase);
        }

        /// <summary>
        /// Returns wether any of the given keys are currently pressed.
        /// </summary>
        public static bool IsAnyKeyDown(params KeyCode[] keycodes)
        {
            return keycodes.Any(Input.GetKey);
        }

        public static Vector2 MousePosition
        {
            get { return new Vector2(Input.mousePosition.x, Input.mousePosition.y); }
        }
        
        public static bool IsOnScreen(Vector3 position)
        {
            Vector3 onScreen = Camera.current.WorldToViewportPoint(position);
            return onScreen.z > 0 && onScreen.x > 0 && onScreen.y > 0 && onScreen.x < 1 && onScreen.y < 1;
        }

        public static string GetDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
        
        public static Bounds GetMeshBounds(GameObject go)
        {
            var comps = go.GetComponentsInChildren<Renderer>();
            return comps.GetCombinedBounds();
        }

        public static Bounds GetColliderBounds(GameObject go)
        {
            var comps = go.GetComponentsInChildren<Collider>();
            return comps.GetCombinedBounds();
        }

        public static BoxCollider WrapRectTransformInCollider(RectTransform t, float zSize = .05f)
            => WrapRectTransformInCollider(t, Vector2.one, zSize);

        public static BoxCollider WrapRectTransformInCollider(RectTransform t, Vector2 scale, float zSize = .05f)
        {
            Vector2 pivot = t.pivot;
            float zScale = zSize / t.localScale.z;
            Vector2 size = t.rect.size; // t.sizeDelta instead?
            
            var collider = t.gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(size.x * scale.x, size.y * scale.y, zScale);
            collider.center = new Vector3(size.x / 2 - size.x * pivot.x, size.y / 2 - size.y * pivot.y, zScale / 2f);

            return collider;
        }
        
        /// Use this method to get all loaded scene objects of some type, including inactive objects. 
        /// <para>This is an alternative to Resources.FindObjectsOfTypeAll (returns project assets, including prefabs), and GameObject.FindObjectsOfTypeAll (deprecated).</para>
        public static void FindSceneObjectsOfTypeAll<T>(ICollection<T> results)
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

        private static void FindObjectsInScene<T>(Scene s, ref ICollection<T> results)
        {
            if (!s.isLoaded) return;
            
            GameObject[] rootObjects = s.GetRootGameObjects();
            for (int j = 0; j < rootObjects.Length; j++)
            {
                var go = rootObjects[j];
                if (results is ICollection<GameObject> typedResults)
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
            while (foundPath != null && foundPath.Contains('/'))
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

        public static int GetHashCode(params object[] args)
        {
            if (args.Length == 1) return args[0].GetHashCode();
            int hashCode = 0x00FF;
            
            for (var i = 0; i < args.Length; i++)
                hashCode ^= args[i].GetHashCode();

            return hashCode;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
    }
}