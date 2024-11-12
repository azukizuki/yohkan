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
    public partial class YohkanAssetProvider :  IAssetContainer , IDisposable
    {
        private readonly List<AssetInfo> _cachedAssets = new();
        private readonly HashSet<AssetReserveInfo> _reserveAssets = new();
        private readonly IAssetResolveEvent _resolveEvent = null;
        private readonly Dictionary<string, Sprite> _cachedSpriteDict = new();


        public YohkanAssetProvider(IAssetResolveEvent resolveEvent = null)
        {
            _cachedAssets?.Clear();
            _reserveAssets?.Clear();
            _cachedSpriteDict?.Clear();
            _resolveEvent = resolveEvent;
        }
        
        public void Dispose()
        {
            foreach (var cached in _cachedAssets)
            {
                Addressables.Release(cached.Asset);
            }

            foreach (var kvp in _cachedSpriteDict)
            {
                if (kvp.Value != null)
                {
                    Object.Destroy(kvp.Value);
                }
            }
            _cachedAssets.Clear();
            _reserveAssets.Clear();
            _cachedSpriteDict.Clear();
        }
    }
}
