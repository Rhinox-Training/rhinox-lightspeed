using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rhinox.Lightspeed.IO;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Rhinox.Lightspeed.Addressables
{
    public static class AddressablesExt
    {
#if UNITY_EDITOR
        private static string _binFilePath;

        public static AddressablesPlayerBuildResult BuildOrUpdatePlayerContent()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            AddressableLabelTable.CreateContentUpdate();
            if (File.Exists(_binFilePath))
            {
                //Update catalog
                return ContentUpdateScript.BuildContentUpdate(settings, _binFilePath);
            }
            else
            {
                AddressableAssetSettings.BuildPlayerContent(out var result);
                _binFilePath = ContentUpdateScript.GetContentStateDataPath(false);
                return result;
            }
        }

        public static string GetTargetBuildPath()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var buildPathKey = settings.RemoteCatalogBuildPath;
            string path = buildPathKey.GetValue(settings);
            return FileHelper.GetFullPath(path, FileHelper.GetProjectPath());
        }
        
        public static ICollection<AddressableAssetEntry> FindAssetPathsWithLabelLocal(string label, string groupName = null)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            var group = settings.FindGroup(groupName);
            if (groupName == null || group == null)
                group = settings.DefaultGroup;

            return group.entries
                .Where(x => x.labels.Any(lbl => lbl.Equals(label, StringComparison.InvariantCultureIgnoreCase)))
                .ToArray();
        }
#endif
        
        public static AsyncOperationHandle<IReadOnlyCollection<string>> LoadCatalogLabelsAsync()
        {
            var op = new LoadLabelTableOperation();
            return UnityEngine.AddressableAssets.Addressables.ResourceManager.StartOperation(op, default);
        }
    }
}