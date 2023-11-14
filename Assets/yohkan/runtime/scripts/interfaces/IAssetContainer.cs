using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace yohkan.runtime.scripts.interfaces
{
    public interface IAssetContainer
    {
        T GetAsset<T>(string address) where T : UnityEngine.Object;
        IEnumerable<T> GetAssets<T>(IEnumerable<string> addresses) where T : UnityEngine.Object;
        T GetAsset<T>(AssetReference reference) where T : UnityEngine.Object;
        IEnumerable<T> GetAssets<T>(IEnumerable<AssetReference> references) where T : UnityEngine.Object;
    }
}
