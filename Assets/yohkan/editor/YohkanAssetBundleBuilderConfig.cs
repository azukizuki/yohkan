using UnityEngine;
using UnityEngine.Serialization;

namespace yohkan.editor
{
    [CreateAssetMenu(menuName = YohkanEditorConst.CREATE_ASSET_MENU_PREFIX + "AssetBundleBuilderConfig")]
    public class YohkanAssetBundleBuilderConfig : ScriptableObject
    {
        [SerializeField] private string _contentStateBinaryRootDir = string.Empty;
        public string ContentStateBinaryRootDir
        {
            get => _contentStateBinaryRootDir;
            set => _contentStateBinaryRootDir = value;
        }

        [SerializeField] private string _remoteBuildRootPath = string.Empty;
        public string RemoteBuildRootPath
        {
            get => _remoteBuildRootPath;
            set => _remoteBuildRootPath = value;
        }

        [SerializeField] private string _remoteLoadRootPath = string.Empty;
        public string RemoteLoadRootPath
        {
            get => _remoteLoadRootPath;
            set => _remoteLoadRootPath = value;
        }

        [SerializeField] private bool _enableYohkanAddressableLabelSetWhenBundleBuild = true;
        public bool EnableYohkanAddressableLabelSetWhenBundleBuild
        {
            get => _enableYohkanAddressableLabelSetWhenBundleBuild;
            set => _enableYohkanAddressableLabelSetWhenBundleBuild = value;
        }
    }
}
