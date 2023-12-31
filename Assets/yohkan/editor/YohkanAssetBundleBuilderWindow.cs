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

        private string _buildSuffix = string.Empty;

        private void Init()
        {
        }

        private void OnGUI()
        {
            _buildSuffix = EditorGUILayout.TextField("BundleSuffix", _buildSuffix);

            if (GUILayout.Button("新規ビルド"))
            {
                YohkanAssetBundleBuilder.BuildAssetBundle(_buildSuffix);
            }

            if (GUILayout.Button("差分ビルド"))
            {
                YohkanAssetBundleBuilder.BuildWithContentState(_buildSuffix);
            }
        }
    }
}
