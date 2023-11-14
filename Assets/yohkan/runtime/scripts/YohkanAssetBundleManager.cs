using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using yohkan.runtime.scripts.interfaces;
using Object = UnityEngine.Object;

namespace yohkan.runtime.scripts
{
    public class YohkanAssetBundleManager : IDisposable
    {

        private readonly List<IDisposable> _bundleProviderDisposables = new();
        
        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            Debug.Log("check catalog");
            await Addressables.InitializeAsync().Task;
        
            var res = await Addressables.CheckForCatalogUpdates(true).Task;
            if (res.Any())
            {
                Debug.Log("Detected Catalog Update!");
                await Addressables.UpdateCatalogs(res, true).Task;
                Debug.Log("Updated Internal Catalogs!");
            }
            else
            {
                Debug.Log("non detect catalog update.");
            }
        }
        
        public async Task DownloadAllRemoteAssetsAsync(Func<long,Task<bool>> askDownloadConfirmEvent,Action<float> onUpdateDownloadProgressEvent, CancellationToken cancellationToken)
        {
            using var provider = CreateAssetBundleProvider(false);
            ((IAssetReserver)provider).ReserveAsset<Object>(YohkanRuntimeConsts.ALL_DOWNLOAD_LABEL);
            await ((IAssetResolver)provider).ResolveAsync(askDownloadConfirmEvent, onUpdateDownloadProgressEvent,
                cancellationToken);
            provider.Dispose();
        }

        public YohkanAssetProvider CreateAssetBundleProvider(bool addLifeManagement = true)
        {
            var instance = new YohkanAssetProvider();
            if (addLifeManagement)
            {
                _bundleProviderDisposables.Add(instance);
            }

            return instance;
        }
        
        public async Task<bool> IsAssetExistByCache(string address)
        {
            var res = await Addressables.GetDownloadSizeAsync(address).Task;
            return res == 0;
        }
        
        public void Dispose()
        {
            foreach (var disposable in _bundleProviderDisposables)
            {
                disposable?.Dispose();
            }
            _bundleProviderDisposables.Clear();
        }
    }
}
