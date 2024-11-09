using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using yohkan.runtime.scripts.debug;
using yohkan.runtime.scripts.interfaces;
using yohkan.runtime.scripts.internals;

namespace yohkan.runtime.scripts
{
    public partial class YohkanAssetProvider : IAssetReserver
    {
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
        
    }
}
