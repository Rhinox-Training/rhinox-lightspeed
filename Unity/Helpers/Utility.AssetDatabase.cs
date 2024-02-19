using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
#if UNITY_EDITOR
        public static string[] FindAssetGuidsByFilter(string filter, params string[] folders)
        {
            return AssetDatabase.FindAssets(filter, folders);
        }

        public static string[] FindAssetGuids<T>(params string[] folders) where T : UnityEngine.Object
            => FindAssetGuidsByFilter($"t:{typeof(T).Name}", folders);
        
        public static T LoadAssetFromGuid<T>(string guid) where T : UnityEngine.Object
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static T[] AssetsFromGuids<T>(params string[] guids) where T : UnityEngine.Object
        {
            var arr = new T[guids.Length];
            for (var i = 0; i < guids.Length; i++)
                arr[i] = LoadAssetFromGuid<T>(guids[i]);
            return arr;
        }
        
        public static T[] FindAssetsByFilter<T>(string filter, params string[] folders) where T : UnityEngine.Object
        {
            string[] guids = FindAssetGuidsByFilter(filter, folders);
            return AssetsFromGuids<T>(guids);
        }
        
        public static T[] FindAssets<T>(params string[] folders) where T : UnityEngine.Object
        {
            string[] guids = FindAssetGuids<T>(folders);
            return AssetsFromGuids<T>(guids);
        }
        
        public static T FindAssetApproximately<T>(params string[] possibleNames) where T : UnityEngine.Object
        {
            var typeFilter = $"t:{typeof(T).Name}";
            foreach (var name in possibleNames)
            {
                string[] guids = AssetDatabase.FindAssets($"{typeFilter} {name}");
                foreach (var guid in guids)
                    return LoadAssetFromGuid<T>(guid);
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

            string[] guids = AssetDatabase.FindAssets("t:prefab", folders.Length == 0 ? null : folders);

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
#endif
    }
}