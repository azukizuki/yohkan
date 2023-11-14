using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace yohkan.runtime.scripts.interfaces
{
    public interface IAssetReserver
    {
        void ReserveAsset<T>(string address) where T : UnityEngine.Object;
        void ReserveAsset<T>(IEnumerable<string> addresses) where T : UnityEngine.Object;
        
        void ReserveAsset<T>(AssetReference reference) where T : UnityEngine.Object;
        void ReserveAsset<T>(IEnumerable<AssetReference> references) where T : UnityEngine.Object;

    }
}
