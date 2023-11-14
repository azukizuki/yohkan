using System;
using UnityEngine.AddressableAssets;

namespace yohkan.runtime.scripts.internals
{
    internal struct AssetReserveInfo
    {
        public string Address;
        public AssetReference Reference;
        public Type AssetType;

        public override int GetHashCode()
        {
            return (Address?.GetHashCode() ?? 0) ^ AssetType.GetHashCode() ^ (Reference?.GetHashCode() ?? 0);
        }

        public override bool Equals(object obj)
        {
            AssetReserveInfo reserveInfo = obj is AssetReserveInfo info ? info : default;
            return Address == reserveInfo.Address && AssetType == reserveInfo.AssetType && Reference == reserveInfo.Reference;
        }
    }
}
