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
using yohkan.runtime.scripts.internals;
using Object = UnityEngine.Object;
#if YOHKAN_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace yohkan.runtime.scripts
{
    public class YohkanAssetProvider : IAssetReserver , IAssetResolver , IAssetContainer , IDisposable
    {
        private readonly List<AssetInfo> _cachedAssets = new();
        private readonly HashSet<AssetReserveInfo> _reserveAssets = new();
        private readonly IAssetResolveEvent _resolveEvent = null;


        public YohkanAssetProvider(IAssetResolveEvent resolveEvent = null)
        {
            _cachedAssets?.Clear();
            _reserveAssets?.Clear();
            _resolveEvent = resolveEvent;
        }
        
        void IAssetReserver.ReserveAsset<T>(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                YohkanLogger.LogWarning("address is Empty!");
                return;
            }
            _reserveAssets.Add(new AssetReserveInfo()
            {
                Address = address,
                AssetType = typeof(T)
            });
        }

        void IAssetReserver.ReserveAsset<T>(IEnumerable<string> addresses)
        {
            foreach (var address in addresses)
            {
                (this as IAssetReserver).ReserveAsset<T>(address);
            }
        }

        void IAssetReserver.ReserveAsset<T>(AssetReference reference)
        {
            if (reference == null) return;

            _reserveAssets.Add(new AssetReserveInfo()
            {
                Reference = reference,
                AssetType = typeof(T)
            });
        }

        void IAssetReserver.ReserveAsset<T>(IEnumerable<AssetReference> references)
        {
            foreach (var reference in references)
            {
                (this as IAssetReserver).ReserveAsset<T>(reference);
            }
        }
        
        #if YOHKAN_ENABLE_UNITASK
        async UniTask IAssetResolver.ResolveAsync(CancellationToken cancellationToken)
        #else
        async Task IAssetResolver.ResolveAsync(CancellationToken cancellationToken)
        #endif
        {
            if (!_reserveAssets.Any()) return;

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
            
            var downloadSize = await Addressables.GetDownloadSizeAsync(keys).Task;
            
            if (downloadSize > 0)
            {
                YohkanLogger.Log("Start Download Process");
                var agreement = true;
                if (_resolveEvent != null)
                {
                    agreement = await _resolveEvent.AskDownloadConfirm(downloadSize);
                }

                if (!agreement)
                {
                    throw new Exception("Download Cancelled by user.");
                }

                var downloadTaskResult =
                    Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union, false);
                #if YOHKAN_ENABLE_UNITASK
                await UniTask.WhenAll(downloadTaskResult.ToUniTask(cancellationToken: cancellationToken),
                    PublishDownloadProgressEvent(downloadTaskResult));
                #else
                 await Task.WhenAll(downloadTaskResult.Task,
                    PublishDownloadProgressEvent(downloadTaskResult));
                #endif
                
                Addressables.Release(downloadTaskResult);
            }

            var tasks = _reserveAssets.Select(SetCacheAsync);
            await Task.WhenAll(tasks);
            
            _reserveAssets.Clear();
            YohkanLogger.Log("loaded.");
        }

        #if YOHKAN_ENABLE_UNITASK
        private async UniTask PublishDownloadProgressEvent(AsyncOperationHandle handle)
        #else
        private async Task PublishDownloadProgressEvent(AsyncOperationHandle handle)
        #endif
        {
            while (true)
            {
                if (!handle.IsValid()) break;
                if (handle.Status == AsyncOperationStatus.Failed) break;
                if (handle.PercentComplete >= 1f || handle.Status == AsyncOperationStatus.Succeeded) break;
                
                _resolveEvent?.OnUpdateDownloadProgress(handle.PercentComplete);
                #if YOHKAN_ENABLE_UNITASK
                await UniTask.Delay(4);
                #else
                await Task.Delay(4);
                #endif
            }
            _resolveEvent?.OnUpdateDownloadProgress(1f);

        }

        private async Task SetCacheAsync(AssetReserveInfo info)
        {
            Object result = null;
            if (!string.IsNullOrWhiteSpace(info.Address))
            {
                if (_cachedAssets.Any(m => m.Address == info.Address && m.AssetType == info.AssetType)) return;
                
                result = await Addressables.LoadAssetAsync<UnityEngine.Object>(info.Address).Task;
                _cachedAssets.Add(new AssetInfo()
                {
                    Address = info.Address,
                    Asset = result,
                    AssetType = info.AssetType,
                    Reference = info.Reference
                });
                return;
            }

            if (info.Reference == null) return;
            
            if (_cachedAssets.Any(m => m.Reference == info.Reference && m.AssetType == info.AssetType)) return;

            result = await Addressables.LoadAssetAsync<UnityEngine.Object>(info.Reference).Task;
            _cachedAssets.Add(new AssetInfo()
            {
                Address = info.Address,
                Asset = result,
                AssetType = info.AssetType,
                Reference = info.Reference
            });
            return;
        }
        
        T IAssetContainer.GetAsset<T>(string address)
        {
            var type = typeof(T);
            AssetInfo loadTarget = default;
            bool findTarget = false;
            foreach (var cache in _cachedAssets)
            {
                if (cache.Address == address && type == cache.AssetType)
                {
                    loadTarget = cache;
                    findTarget = true;
                    break;
                }
            }

            if (!findTarget)
            {
                YohkanLogger.LogError($"Not Found by ResolvedAsset: {address}.");
                return null;
            }

            var convertedAsset = loadTarget.Asset as T;
            
            if (convertedAsset == null)
            {
                if (type == typeof(Sprite))
                {
                    var sprite = ConvertTexture2Sprite(loadTarget.Asset as Texture2D);
                    if (sprite != null)
                    {
                        return sprite as T;
                    }
                }
                
                YohkanLogger.LogError("Asset Type is MissMatch!");
            }

            return convertedAsset;
        }

        
        IEnumerable<T> IAssetContainer.GetAssets<T>(IEnumerable<string> addresses)
        {
            return addresses.Select(m => (this as IAssetContainer).GetAsset<T>(m));
        }

        T IAssetContainer.GetAsset<T>(AssetReference reference)
        {
            var type = typeof(T);
            AssetInfo loadTarget = default;
            bool findTarget = false;
            foreach (var cache in _cachedAssets)
            {
                if (cache.Reference == reference && type == cache.AssetType)
                {
                    loadTarget = cache;
                    findTarget = true;
                    break;
                }
            }

            if (!findTarget)
            {
                YohkanLogger.LogError($"Not Found by ResolvedAsset: {reference.AssetGUID}.");
                return null;
            }

            var convertedAsset = loadTarget.Asset as T;
            
            if (convertedAsset == null)
            {
                if (type == typeof(Sprite) && loadTarget.AssetType == typeof(Texture2D))
                {
                    var sprite = ConvertTexture2Sprite(loadTarget.Asset as Texture2D);
                    if (sprite != null)
                    {
                        return sprite as T;
                    }
                }
                
                YohkanLogger.LogError("Asset Type is MissMatch!");
            }

            return convertedAsset;
        }

        IEnumerable<T> IAssetContainer.GetAssets<T>(IEnumerable<AssetReference> references)
        {
            return references.Select(m => (this as IAssetContainer).GetAsset<T>(m));
        }
        
        private Sprite ConvertTexture2Sprite(Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), Vector2.zero);
        }

        public void Dispose()
        {
            foreach (var cached in _cachedAssets)
            {
                Addressables.Release(cached.Asset);
            }
            _cachedAssets.Clear();
            _reserveAssets.Clear();
        }
    }
}
