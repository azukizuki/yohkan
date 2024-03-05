using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.Device;

namespace yohkan.editor
{
    internal static class YohkanUtil
    {
        public static AddressableAssetSettings GetSettings()
        {
            return AddressableAssetSettingsDefaultObject.Settings;
        }

        public static string CreateRemoteLoadPath(string suffix,string remoteLoadRootPath = null)
        {
            var useConfig = string.IsNullOrEmpty(remoteLoadRootPath);
            var config = GetBuilderConfig();
            return Path.Combine((useConfig ? config.RemoteLoadRootPath : remoteLoadRootPath), CreateRemoteCatalogVersionString(suffix),
                EditorUserBuildSettings.activeBuildTarget.ToString());
        }

        public static string CreateRemoteBuildPath(string suffix)
        {
            var config = GetBuilderConfig();
            return Path.Combine(config.RemoteBuildRootPath, CreateRemoteCatalogVersionString(suffix),
                EditorUserBuildSettings.activeBuildTarget.ToString());
            
        }

        public static string CreateRemoteCatalogVersionString(string suffix)
        {
            return Application.version + "_" + suffix;
        }

        public static string CreateContentStateBuildPathString(string suffix)
        {
            var config = GetBuilderConfig();
            return  Path.Combine(Application.dataPath,
                config.ContentStateBinaryRootDir,CreateRemoteCatalogVersionString(suffix),EditorUserBuildSettings.activeBuildTarget.ToString());
        }
        
        public static YohkanAssetBundleBuilderConfig GetBuilderConfig()
        {
            var configs = AssetDatabase.FindAssets($"t:{nameof(YohkanAssetBundleBuilderConfig)}");
            if (configs == null || configs?.Length > 1)
            {
                throw new Exception($"{nameof(YohkanAssetBundleBuilderConfig)}がプロジェクトに存在しない　または複数個存在します");
            }

            var path = AssetDatabase.GUIDToAssetPath(configs[0]);
            return AssetDatabase.LoadAssetAtPath<YohkanAssetBundleBuilderConfig>(path);
        }
    }
}
