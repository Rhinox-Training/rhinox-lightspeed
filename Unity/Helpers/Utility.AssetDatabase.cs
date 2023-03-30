using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
#if UNITY_EDITOR
        public static T FindApproximately<T>(params string[] possibleNames) where T : UnityEngine.Object
        {
            foreach (var name in possibleNames)
            {
                string[] guids = AssetDatabase.FindAssets(name);
                foreach (var guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    // NOTE: Unity is stupid, so we need to load every variant as a potential typed instance to check if it is of a certain type
                    var potentialInstance = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    if (potentialInstance != null)
                    {
                        return potentialInstance;
                    }
                }
            }

            return null;
        }

        public static T[] FindPrefabsOfType<T>(params string[] folders) where T : MonoBehaviour
        {
            if (folders == null)
                folders = Array.Empty<string>();

            if (folders.Length > 0)
                folders = folders.Where(x => x != null).ToArray();

            List<T> prefabMonoBehaviours = new List<T>();

            //AssetDatabase.Refresh();

            string[] guids = AssetDatabase.FindAssets("t:Object", folders.Length == 0 ? new[] { "Assets" } : folders);

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

#endif
    }
}