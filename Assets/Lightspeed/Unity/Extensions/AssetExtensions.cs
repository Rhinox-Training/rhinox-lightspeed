namespace Rhinox.Lightspeed
{
#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public static class AssetExtensions
    {
        public static T[] FindPrefabsOfType<T>(params string[] folders) where T : MonoBehaviour
        {
            //AssetDatabase.Refresh();
            if (folders == null)
                folders = Array.Empty<string>();
            if (folders.Length > 0)
                folders = folders.Where(x => x != null).ToArray();
            string[] guids = AssetDatabase.FindAssets("t:Object", folders.Length == 0 ? new[] { "Assets" } : folders);

            List<T> prefabMonoBehaviours = new List<T>();
            foreach (string guid in guids)
            {
                //Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
                string myObjectPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(myObjectPath);
                if (asset == null)
                    continue;

                if (!PrefabUtility.IsPartOfPrefabAsset(asset))
                    continue;

                T component = asset.GetComponent<T>();
                if (component == null)
                    continue;

                prefabMonoBehaviours.Add(component);
            }

            return prefabMonoBehaviours.ToArray();
        }
    
        public static T[] FindScriptableObjectsOfType<T>(params string[] folders) where T : ScriptableObject
        {
            //AssetDatabase.Refresh();
            string[] assetGUIDs = AssetDatabase.FindAssets($"t:{typeof(T).FullName}", folders.Length == 0 ? new[] { "Assets" } : folders);
        
            List<T> scriptableObjects = new List<T>();
            foreach (string guid in assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset == null)
                    continue;
            
                scriptableObjects.Add(asset);
            }
        
            return scriptableObjects.ToArray();
        }
    }
#endif
}