using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using yohkan.runtime.scripts.debug;
using yohkan.runtime.scripts.interfaces;
using Object = UnityEngine.Object;
#if YOHKAN_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace yohkan.runtime.scripts
{
    public class YohkanAssetBundleManager : IDisposable
    {

        private readonly List<IDisposable> _bundleProviderDisposables = new();
        private readonly Dictionary<Type, IExternalAssetContainer> _externalAssetContainers = new();
        
        #if YOHKAN_ENABLE_UNITASK
        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        #else
        public async Task InitializeAsync(CancellationToken cancellationToken)
        #endif
        {
            //addressableの初期化完了を待つ。初期化完了していない時にカタログ更新をチェックすると更新があっても更新無しになる
            YohkanLogger.Log("[CatalogInitialize]Waiting AAS Initialization.");
            await Addressables.InitializeAsync().Task;
            //カタログ更新あるかチェック。
            YohkanLogger.Log("[CatalogInitialize]Check Catalog Update");
            var catalogUpdateOp = Addressables.CheckForCatalogUpdates(false);
            var catalogIds = await catalogUpdateOp.Task;
            //OperationHandleを開放しておく
            Addressables.Release(catalogUpdateOp);
            //カタログ更新ある場合
            if (catalogIds.Any())
            {
                var sb = new StringBuilder("Detected Catalog Update! CatalogIds:");
                foreach (var catalogId in catalogIds)
                {
                    sb.AppendLine(catalogId);
                }
                YohkanLogger.Log(sb.ToString());
                
                //カタログ更新を実行。機内モードなどで通信失敗した際もoperationHandleがDisposeされてしまい、古いAssetBundleが削除され進行不能になってしまうのでfalseにしておく
                YohkanLogger.Log("[CatalogInitialize]Start Catalog Update process...");
                var op = Addressables.UpdateCatalogs(catalogIds, autoReleaseHandle: false);
                var locators = await op.Task;
                if (!locators.Any())
                {
                    //更新失敗している時は例外を投げる（IResourceLocatorが空かどうかで判定する）
                    //MEMO ここでopのReleaseをしていないのは意図的。ここで開放しちゃうと端末内に元からあったAssetBundle開放されちゃって進行不能になる
                    YohkanLogger.Log("[CatalogInitialize]Failed Catalog Update!!");
                    throw new Exception("Failed Catalog Update!!");
                }
                else
                {
                    YohkanLogger.Log("[CatalogInitialize]Update Success! Release OperationHandle.");
                    //OperationHandleも忘れず開放
                    Addressables.Release(op);
                    //ココに来た時は更新成功しているので古いBundleを消す
                    YohkanLogger.Log("[CatalogInitialize]Clean Bundle Cache.");
                    await Addressables.CleanBundleCache().Task;
                    YohkanLogger.Log("[CatalogInitialize]Completed All Update Process!");
                }
            }
            else
            {
                YohkanLogger.Log("[CatalogInitialize]No Catalog Update.");
            }
        }
        
        #if YOHKAN_ENABLE_UNITASK
        public async UniTask DownloadAllRemoteAssetsAsync(IAssetResolveEvent resolveEvent, CancellationToken cancellationToken)
        #else
        public async Task DownloadAllRemoteAssetsAsync(IAssetResolveEvent resolveEvent, CancellationToken cancellationToken)
        #endif
        {
            await DownloadRemoteAssetByLabel(resolveEvent, new []{YohkanRuntimeConsts.ALL_DOWNLOAD_LABEL}, cancellationToken);
        }

        #if YOHKAN_ENABLE_UNITASK
        public async UniTask DownloadRemoteAssetByLabel(IAssetResolveEvent resolveEvent, IEnumerable<string> label,
            CancellationToken cancellationToken)
        #else
        public async Task DownloadRemoteAssetByLabel(IAssetResolveEvent resolveEvent, IEnumerable<string> label,
            CancellationToken cancellationToken)
        #endif
        {
            #if YOHKAN_ENABLE_UNITASK
            async UniTask PublishDownloadProgressEvent(AsyncOperationHandle handle,IAssetResolveEvent re)
            #else
            async Task PublishDownloadProgressEvent(AsyncOperationHandle handle,IAssetResolveEvent re)
            #endif
            {
                while (true)
                {
                    if (!handle.IsValid()) break;
                    if (handle.Status == AsyncOperationStatus.Failed) break;
                    if (handle.PercentComplete >= 1f || handle.Status == AsyncOperationStatus.Succeeded) break;
                
                    re?.OnUpdateDownloadProgress(handle.PercentComplete);
                    #if YOHKAN_ENABLE_UNITASK
                    await UniTask.Delay(4, cancellationToken: cancellationToken);
                    #else
                    await Task.Delay(4);
                    #endif
                }
                re?.OnUpdateDownloadProgress(1f);
            }
            
            var downloadSizeOperation = Addressables.GetDownloadSizeAsync(label);
            var downloadSize = await downloadSizeOperation.Task;
            if (downloadSizeOperation.Status != AsyncOperationStatus.Succeeded && downloadSizeOperation.OperationException != null)
            {
                throw downloadSizeOperation.OperationException;
            }
            
            if (downloadSize > 0)
            {
                YohkanLogger.Log("Start Download Process");
                var agreement = true;
                if (resolveEvent != null)
                {
                    agreement = await resolveEvent.AskDownloadConfirmAsync(downloadSize,cancellationToken);
                }

                if (!agreement)
                {
                    throw new Exception("Download Cancelled by user.");
                }

                if (resolveEvent != null)
                {
                    await resolveEvent.OnStartDownloadAsync(cancellationToken);
                }

                try
                {
                    var downloadTaskResult =
                        Addressables.DownloadDependenciesAsync(label, Addressables.MergeMode.Union, false);
#if YOHKAN_ENABLE_UNITASK
                    await UniTask.WhenAll(downloadTaskResult.ToUniTask(cancellationToken: cancellationToken),
                        PublishDownloadProgressEvent(downloadTaskResult, resolveEvent));
#else
                    await Task.WhenAll(downloadTaskResult.Task,
                        PublishDownloadProgressEvent(downloadTaskResult, resolveEvent));
#endif
                    if (downloadTaskResult.Status != AsyncOperationStatus.Succeeded && downloadTaskResult.OperationException != null)
                    {
                        throw downloadTaskResult.OperationException;
                    }
                    
                    Addressables.Release(downloadTaskResult);
                }
                catch (OperationCanceledException)
                {
                    YohkanLogger.LogWarning("Download Cancelled!");
                    throw;
                }
                catch (Exception e)
                {
                    YohkanLogger.LogError($"DownloadFailed. {e.Message + e.StackTrace}");
                    throw;
                }
                finally
                {
                    if (resolveEvent != null)
                    {
                        await resolveEvent.OnEndDownloadAsync(cancellationToken);
                    }
                }
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
        
        #if YOHKAN_ENABLE_UNITASK
        public async UniTask<bool> IsAssetExistByCache(string address)
        #else
        public async Task<bool> IsAssetExistByCache(string address)
        #endif
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
