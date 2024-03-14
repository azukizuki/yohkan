using System;
using UnityEditor;
using UnityEngine;

namespace yohkan.editor
{
    public class YohkanAssetBundleBuilderWindow : EditorWindow
    {

        [MenuItem(YohkanEditorConst.BUILD_MENU_ITEM_PREFIX + "AssetBundleBuildWindow")]
        public static void OpenWindow()
        {
            var currentWindow = GetWindow<YohkanAssetBundleBuilderWindow>();
            if (currentWindow != null)
            {
                currentWindow.Close();
            }

            var instance = CreateWindow<YohkanAssetBundleBuilderWindow>(nameof(YohkanAssetBundleBuilderWindow));
            instance.Init();
        }

        private string _resourceUniqueString = string.Empty;

        private void Init()
        {
        }

        private void OnGUI()
        {
            _resourceUniqueString = EditorGUILayout.TextField("resourceUniqueString", _resourceUniqueString);

            if (GUILayout.Button("新規ビルド"))
            {
                YohkanAssetBundleBuilder.BuildAssetBundle(new YohkanAssetBundleBuilder.AssetBundleBuildParameter(_resourceUniqueString,null));
            }

            if (GUILayout.Button("差分ビルド"))
            {
                YohkanAssetBundleBuilder.BuildWithContentState(new YohkanAssetBundleBuilder.AssetBundleBuildParameter(_resourceUniqueString,null));
            }
        }
    }
}
