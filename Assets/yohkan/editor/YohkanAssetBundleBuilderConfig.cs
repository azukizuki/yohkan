using UnityEngine;
using UnityEngine.Serialization;

namespace yohkan.editor
{
    [CreateAssetMenu(menuName = YohkanEditorConst.CREATE_ASSET_MENU_PREFIX + "AssetBundleBuilderConfig")]
    public class YohkanAssetBundleBuilderConfig : ScriptableObject
    {
        [SerializeField] private string _contentStateBinaryRootDir = string.Empty;
        public string ContentStateBinaryRootDir => _contentStateBinaryRootDir;

        [SerializeField] private string _remoteBuildRootPath = string.Empty;
        public string RemoteBuildRootPath => _remoteBuildRootPath;
        
        [SerializeField] private string _remoteLoadRootPath = string.Empty;
        public string RemoteLoadRootPath => _remoteLoadRootPath;

        [SerializeField] private bool _enableYohkanAddressableLabelSetWhenBundleBuild = true;
        public bool EnableYohkanAddressableLabelSetWhenBundleBuild => _enableYohkanAddressableLabelSetWhenBundleBuild;
        
    }
}
