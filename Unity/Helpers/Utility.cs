using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Reflection;
using UnityEngine.Rendering;
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

        public static Material GetDefaultMaterial(MaterialType type = MaterialType.Metal)
        {
            // Handle build-in pipeline
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                switch (type)
                {
                    default:
                        // TODO
                        var shader = Shader.Find("Standard");
                        return new Material(shader);
                }
            }

            // Handle SRP
            switch (type)
            {
                case MaterialType.Default:
                    return GraphicsSettings.currentRenderPipeline.defaultMaterial;
                case MaterialType.Metal:
                    // TODO set metallic value to 1
                    return GraphicsSettings.currentRenderPipeline.defaultMaterial;
                case MaterialType.UI:
                    return GraphicsSettings.currentRenderPipeline.defaultUIMaterial;
                // TODO
                default:
                    return GraphicsSettings.currentRenderPipeline.defaultMaterial;
            }
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
        
        /// <summary>
        /// Does a raycast from where you clicked, infinite length
        /// </summary>
        public static bool ClickRaycast(out RaycastHit hitInfo, LayerMask mask)
        {
            var mousePos = Utility.GetMousePosition();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            return Physics.Raycast(ray, out hitInfo, Mathf.Infinity, mask);
        }

        /// <summary>
        /// Does a raycast from where you clicked, infinite length, any layer
        /// </summary>
        public static bool ClickRaycast(out RaycastHit hitInfo)
        {
            var mousePos = Utility.GetMousePosition();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            return Physics.Raycast(ray, out hitInfo);
        }
        
        public static bool IsOnScreen(Vector3 position)
        {
            Vector3 onScreen = Camera.current.WorldToViewportPoint(position);
            return onScreen.z > 0 && onScreen.x > 0 && onScreen.y > 0 && onScreen.x < 1 && onScreen.y < 1;
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

        private static Version _runtimeVersion;
        public static Version GetCurrentUnityRuntime()
        {
            if (_runtimeVersion == null)
            {
                string unityVersionStr = Application.unityVersion;
                int index = unityVersionStr.IndexOf("f", StringComparison.InvariantCultureIgnoreCase);
                if (index != -1)
                    unityVersionStr = unityVersionStr.Substring(0, index);

                _runtimeVersion = new Version(unityVersionStr);
            }
            return _runtimeVersion;
        }
    }
}