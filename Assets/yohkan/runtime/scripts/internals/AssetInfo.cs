using System;
using UnityEngine.AddressableAssets;

namespace yohkan.runtime.scripts.internals
{
    internal struct AssetInfo
    {
        public string Address;
        public AssetReference Reference;
        public UnityEngine.Object Asset;
        public UnityEngine.Sprite SpriteAsset;//　Texture2d -> Sprite変換が必要になるのでSpriteだけ例外的にフィールドを分ける
        public Type AssetType;
    }
}
