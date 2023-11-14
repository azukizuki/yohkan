using System;
using UnityEngine.AddressableAssets;

namespace yohkan.runtime.scripts.internals
{
    internal struct AssetInfo
    {
        public string Address;
        public AssetReference Reference;
        public UnityEngine.Object Asset;
        public Type AssetType;
    }
}
