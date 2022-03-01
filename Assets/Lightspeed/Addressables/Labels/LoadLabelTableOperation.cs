using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Rhinox.Lightspeed.Addressables
{
    internal class LoadLabelTableOperation : AsyncOperationBase<IReadOnlyCollection<string>>
    {
        private readonly string _key;
        private AsyncOperationHandle<AddressableLabelTable> handle;
     
        public LoadLabelTableOperation()
        {
            _key = ToForcedRemotePath(AddressableLabelTable.TABLE_STORE_PATH);
        }

        private static string ToForcedRemotePath(string s)
        {
            s = s.ToLowerInvariant();
            // NOTE:
            // The remote filepath of addressable assets is prefixed with 'remoteassets_assets_'
            // (Found through trial and error)
            // TODO: is this future proof?
            return s.Replace("assets/", "remoteassets_assets_assets/") + ".bundle";
        }
     
        protected override void Execute()
        {
            handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<AddressableLabelTable>(_key);
            handle.Completed += x =>
            {
                if (x.Status == AsyncOperationStatus.Succeeded)
                    base.Complete(x.Result.Labels, true, string.Empty);
                else
                    base.Complete(Array.Empty<string>(), false, "AssetNotFound");
            };
        }
     
        protected override void Destroy()
        {
            base.Destroy();
     
            if (handle.IsValid())
                UnityEngine.AddressableAssets.Addressables.Release(handle);
        }
    }
}