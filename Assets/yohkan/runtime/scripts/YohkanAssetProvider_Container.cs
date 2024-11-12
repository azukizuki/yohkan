using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using yohkan.runtime.scripts.debug;
using yohkan.runtime.scripts.interfaces;
using yohkan.runtime.scripts.internals;

namespace yohkan.runtime.scripts
{
    public partial class YohkanAssetProvider : IAssetContainer
    {
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
                    return loadTarget.SpriteAsset as T;
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
                if (type == typeof(Sprite))
                {
                    return loadTarget.SpriteAsset as T;
                }
                
                YohkanLogger.LogError("Asset Type is MissMatch!");
            }

            return convertedAsset;
        }

        IEnumerable<T> IAssetContainer.GetAssets<T>(IEnumerable<AssetReference> references)
        {
            return references.Select(m => (this as IAssetContainer).GetAsset<T>(m));
        }
        
      
    }
}
