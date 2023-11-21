using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using yohkan.runtime.scripts.debug;
using yohkan.runtime.scripts.interfaces;
using Object = UnityEngine.Object;

namespace yohkan.runtime.scripts
{
    public class YohkanAssetBundleManager : IDisposable
    {

        private readonly List<IDisposable> _bundleProviderDisposables = new();
        private readonly Dictionary<Type, IExternalAssetContainer> _externalAssetContainers = new();
        
        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            YohkanLogger.Log("check catalog");
            await Addressables.InitializeAsync().Task;
        
            var res = await Addressables.CheckForCatalogUpdates(true).Task;
            if (res.Any())
            {
                YohkanLogger.Log("Detected Catalog Update!");
                await Addressables.UpdateCatalogs(res, true).Task;
                YohkanLogger.Log("Updated Internal Catalogs!");
            }
            else
            {
                YohkanLogger.Log("non detect catalog update.");
            }
        }
        
        public async Task DownloadAllRemoteAssetsAsync(IAssetResolveEvent resolveEvent, CancellationToken cancellationToken)
        {
            await DownloadRemoteAssetByLabel(resolveEvent, new []{YohkanRuntimeConsts.ALL_DOWNLOAD_LABEL}, cancellationToken);
        }

        public async Task DownloadRemoteAssetByLabel(IAssetResolveEvent resolveEvent, IEnumerable<string> label,
            CancellationToken cancellationToken)
        {
            async Task PublishDownloadProgressEvent(AsyncOperationHandle handle,IAssetResolveEvent re)
            {
                while (true)
                {
                    if (!handle.IsValid()) break;
                    if (handle.Status == AsyncOperationStatus.Failed) break;
                    if (handle.PercentComplete >= 1f || handle.Status == AsyncOperationStatus.Succeeded) break;
                
                    re?.OnUpdateDownloadProgress(handle.PercentComplete);
                    await Task.Delay(4);
                }
                re?.OnUpdateDownloadProgress(1f);
            }
            
            var downloadSize = await Addressables.GetDownloadSizeAsync(label).Task;
            
            if (downloadSize > 0)
            {
                YohkanLogger.Log("Start Download Process");
                var agreement = true;
                if (resolveEvent != null)
                {
                    agreement = await resolveEvent.AskDownloadConfirm(downloadSize);
                }

                if (!agreement)
                {
                    throw new Exception("Download Cancelled by user.");
                }

                var downloadTaskResult =
                    Addressables.DownloadDependenciesAsync(label, Addressables.MergeMode.Union, true);
                await Task.WhenAll(downloadTaskResult.Task,
                    PublishDownloadProgressEvent(downloadTaskResult, resolveEvent));
            }
        }

        public YohkanAssetProvider CreateAssetBundleProvider(IAssetResolveEvent resolveEvent ,bool addLifeManagement = true)
        {
            var instance = new YohkanAssetProvider(resolveEvent);
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

        public IAssetContainer GetExternalContainer<T>() where T : IExternalAssetContainer
        {
            var type = typeof(T);
            if (!_externalAssetContainers.TryGetValue(type, out var container))
            {
                YohkanLogger.LogError($"{type.Name} container not found!");
                return null;
            }

            return container.Container;
        }
        
        public void RegisterContainer<T>(T container) where T : IExternalAssetContainer
        {
            var type = typeof(T);
            if (_externalAssetContainers.ContainsKey(type))
            {
                YohkanLogger.LogWarning($"Already exists {type.Name}");
                return;
            }
            _externalAssetContainers[type] = container;
        }

        public void UnRegisterContainer<T>() where T : IExternalAssetContainer
        {
            var type = typeof(T);
            if (!_externalAssetContainers.ContainsKey(type))
            {
                return;
            }

            _externalAssetContainers.Remove(type);
        }

        public void UnRegisterAllContainer()
        {
            _externalAssetContainers.Clear();
        }
        
        public void Dispose()
        {
            _externalAssetContainers.Clear();
            foreach (var disposable in _bundleProviderDisposables)
            {
                disposable?.Dispose();
            }
            _bundleProviderDisposables.Clear();
        }
    }
}
