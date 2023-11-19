using yohkan.runtime.scripts;

namespace DefaultNamespace
{
    public static class AssetBundleManagerHolder
    {
        public static YohkanAssetBundleManager Manager
        {
            get
            {
                _manager ??= new();
                return _manager;
            }
        }

        private static YohkanAssetBundleManager _manager = null;
    }
}
