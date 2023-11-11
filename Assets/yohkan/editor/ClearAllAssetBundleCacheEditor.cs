using UnityEditor;
using UnityEngine;

namespace yohkan.editor
{
    public class ClearAllAssetBundleCacheEditor
    {
        [MenuItem(YohkanEditorConst.TOOL_MENU_ITEM_PREFIX + "ClearAllAssetBundleCache")]
        public static void ClearAllAssetBundleCache()
        {
            Caching.ClearCache(0);
        }
    }
}
