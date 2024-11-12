using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using yohkan.runtime.scripts.debug;
using yohkan.runtime.scripts.interfaces;
using yohkan.runtime.scripts.internals;
using Object = UnityEngine.Object;

namespace yohkan.runtime.scripts
{
    public partial class YohkanAssetProvider : IAssetResolver
    {
#if YOHKAN_ENABLE_UNITASK
        async UniTask IAssetResolver.ResolveAsync(CancellationToken cancellationToken)
#else
        async Task IAssetResolver.ResolveAsync(CancellationToken cancellationToken)
#endif
        {
            await ResolveInternalAsync(_resolveEvent , cancellationToken);
        }

#if YOHKAN_ENABLE_UNITASK
        async UniTask ResolveInternalAsync(IAssetResolveEvent resolveEvent,CancellationToken cancellationToken)
#else
        async Task ResolveInternalAsync(IAssetResolveEvent resolveEvent,CancellationToken cancellationToken)
#endif
        {
            if (!_reserveAssets.Any()) return;

#if ENABLE_YOHKAN_ANALYZER || UNITY_EDITOR
            YohkanReserveAnalyzer.PushReservedInfo(_reserveAssets.Select(m =>
                m.Reference == null ? m.Address : m.Reference.AssetGUID));
#endif
            var keys = _reserveAssets.Select(m =>
            {
                if (!string.IsNullOrWhiteSpace(m.Address))
                {
                    if (_cachedAssets.Any(n => n.Address == m.Address && n.AssetType == m.AssetType)) return null;
                    return (object)m.Address;
                }

                if (_cachedAssets.Any(n => n.Reference == m.Reference && n.AssetType == m.AssetType)) return null;

                return (object)m.Reference;
            }).Where(m => m != null);


            var downloadSizeOperation = Addressables.GetDownloadSizeAsync(keys);
            var downloadSize = await downloadSizeOperation.Task;
            if (downloadSizeOperation.Status != AsyncOperationStatus.Succeeded &&
                downloadSizeOperation.OperationException != null)
            {
                throw downloadSizeOperation.OperationException;
            }

            if (downloadSize > 0)
            {
                YohkanLogger.Log("Start Download Process");
                var agreement = true;
                if (resolveEvent != null)
                {
                    agreement = await resolveEvent.AskDownloadConfirmAsync(downloadSize, cancellationToken);
                }

                if (!agreement)
                {
                    throw new YohkanUserDownloadCancelledException("DownloadCancelled by User.");
                }

                if (resolveEvent != null)
                {
                    await resolveEvent.OnStartDownloadAsync(cancellationToken);
                }

                try
                {
                    var downloadTaskResult =
                        Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union, false);
#if YOHKAN_ENABLE_UNITASK
                    await UniTask.WhenAll(downloadTaskResult.ToUniTask(cancellationToken: cancellationToken),
                        PublishDownloadProgressEvent(resolveEvent,downloadTaskResult));
#else
                 await Task.WhenAll(downloadTaskResult.Task,
                    PublishDownloadProgressEvent(resolveEvent,downloadTaskResult));
#endif
                    if (downloadTaskResult.Status != AsyncOperationStatus.Succeeded &&
                        downloadTaskResult.OperationException != null)
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

            var tasks = _reserveAssets.Select(SetCacheAsync);
            await Task.WhenAll(tasks);

            _reserveAssets.Clear();
            YohkanLogger.Log("loaded.");
        }


#if YOHKAN_ENABLE_UNITASK
        private async UniTask PublishDownloadProgressEvent(IAssetResolveEvent resolveEvent, AsyncOperationHandle handle)
#else
        private async Task PublishDownloadProgressEvent(IAssetResolveEvent resolveEvent,AsyncOperationHandle handle)
#endif
        {
            while (true)
            {
                if (!handle.IsValid()) break;
                if (handle.Status == AsyncOperationStatus.Failed) break;
                if (handle.PercentComplete >= 1f || handle.Status == AsyncOperationStatus.Succeeded) break;

                resolveEvent?.OnUpdateDownloadProgress(handle.PercentComplete);
#if YOHKAN_ENABLE_UNITASK
                await UniTask.Delay(4);
#else
                await Task.Delay(4);
#endif
            }

            resolveEvent?.OnUpdateDownloadProgress(1f);
        }

        private async Task SetCacheAsync(AssetReserveInfo info)
        {

            #region internal_methods
            async Task SetCacheByAddress()
            {
                if (info.AssetType == typeof(Sprite))
                {
                    var sprite = await Addressables.LoadAssetAsync<Sprite>(info.Address).Task;
                    _cachedAssets.Add(new AssetInfo()
                    {
                        Address = info.Address,
                        SpriteAsset = sprite,
                        AssetType = info.AssetType,
                        Reference = info.Reference
                    });
                    return;
                }
                
                var result = await Addressables.LoadAssetAsync<UnityEngine.Object>(info.Address).Task;
                _cachedAssets.Add(new AssetInfo()
                {
                    Address = info.Address,
                    Asset = result,
                    AssetType = info.AssetType,
                    Reference = info.Reference
                });
                
            }
            
            async Task SetCacheByReference()
            {
                if (info.AssetType == typeof(Sprite))
                {
                    var sprite = await Addressables.LoadAssetAsync<Sprite>(info.Reference).Task;
                    _cachedAssets.Add(new AssetInfo()
                    {
                        Address = info.Address,
                        SpriteAsset = sprite,
                        AssetType = info.AssetType,
                        Reference = info.Reference
                    });
                    return;
                }
                
                var result = await Addressables.LoadAssetAsync<UnityEngine.Object>(info.Reference).Task;
                _cachedAssets.Add(new AssetInfo()
                {
                    Address = info.Address,
                    Asset = result,
                    AssetType = info.AssetType,
                    Reference = info.Reference
                });
                
            }
            #endregion
            
            if (!string.IsNullOrWhiteSpace(info.Address))
            {
                if (_cachedAssets.Any(m => m.Address == info.Address && m.AssetType == info.AssetType)) return;
                
                await SetCacheByAddress();
                return;
            }

            if (info.Reference == null) return;

            if (_cachedAssets.Any(m => m.Reference == info.Reference && m.AssetType == info.AssetType)) return;
            
            await SetCacheByReference();
            return;
        }
    }
}
