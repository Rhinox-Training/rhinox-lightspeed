using System.IO;
#if UNITY_EDITOR
using Rhinox.Lightspeed.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
#endif
using UnityEngine;

namespace Rhinox.Lightspeed.Addressables
{
    public class AddressableLabelTable : ScriptableObject
    {
        internal static string TABLE_STORE_PATH = "Assets/AddressableAssetsData/Custom/LabelTable.asset";
        
        public string[] Labels;
        
#if UNITY_EDITOR
        [MenuItem("Window/Asset Management/Addressables/Update LabelTable Asset", priority = 6000)]
        public static void UpdateAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath(TABLE_STORE_PATH, typeof(AddressableLabelTable)) as AddressableLabelTable;
            if (asset == null)
            {
                FileHelper.CreateAssetsDirectory(Path.GetDirectoryName(TABLE_STORE_PATH));
                var instance = ScriptableObject.CreateInstance<AddressableLabelTable>();
                instance.Labels = AddressableAssetSettingsDefaultObject.Settings.GetLabels().ToArray();
                AssetDatabase.CreateAsset(instance, TABLE_STORE_PATH);
            }
            else
            {
                asset.Labels = AddressableAssetSettingsDefaultObject.Settings.GetLabels().ToArray();
                AssetDatabase.SaveAssets();
            }
        }

        public static void CreateContentUpdate()
        {
            UpdateAsset();
            var guid = AssetDatabase.AssetPathToGUID(TABLE_STORE_PATH);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"Could not find LabelTable at path '{TABLE_STORE_PATH}', skipping insert of data in build...");
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
        }
#endif
    }
}