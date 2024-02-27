using System;
using System.Linq;
using System.Text;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace yohkan.editor
{
    public static class YohkanAssetBundleBuilder
    {
        /// <summary>
        /// ビルド時にYohkanへ渡すパラメータ群
        /// </summary>
        public class AssetBundleBuildParameter
        {
            /// <summary>
            /// カタログのSuffix
            /// </summary>
            public readonly string RemoteCatalogSuffix = string.Empty;
            /// <summary>
            /// YohkanのAssetBundleビルドに関する設定が終わった後に呼ばれます。引数で設定を行ったAddressableAssetSettingを渡します。
            /// 必要に応じてここでビルド前の処理をプロジェクトごとで実装できますが、変更内容によっては正常動作しなくなることがあります。
            /// </summary>
            public readonly Action<AddressableAssetSettings> OnPreProcessBuild = null;

            public AssetBundleBuildParameter(string remoteCatalogSuffix,
                Action<AddressableAssetSettings> onPreProcessBuild)
            {
                this.RemoteCatalogSuffix = remoteCatalogSuffix;
                this.OnPreProcessBuild = onPreProcessBuild;
            }
        }

        public static void BuildAssetBundle(AssetBundleBuildParameter parameter)
        {
            SetUpAddressablePaths(parameter.RemoteCatalogSuffix);
            PrepareAssetBuildProcess(parameter.OnPreProcessBuild);
            AddressableAssetSettings.BuildPlayerContent();
        }

        public static void BuildWithContentState(AssetBundleBuildParameter parameter)
        {
            SetUpAddressablePaths(parameter.RemoteCatalogSuffix);
            PrepareAssetBuildProcess(parameter.OnPreProcessBuild);

            var modifiedEntries = ContentUpdateScript.GatherModifiedEntriesWithDependencies(YohkanUtil.GetSettings(),
                ContentUpdateScript.GetContentStateDataPath(false));
            if (modifiedEntries.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("更新が許可されてないアセットに差分があります");
                foreach (var entry in modifiedEntries)
                {
                    sb.AppendLine(entry.Key.address);
                }

                throw new Exception(sb.ToString());
            }
            Debug.Log(ContentUpdateScript.GetContentStateDataPath(false));
            ContentUpdateScript.BuildContentUpdate(YohkanUtil.GetSettings(),
                ContentUpdateScript.GetContentStateDataPath(false));
            
        }

        private static void SetUpAddressablePaths(string remoteCatalogSuffix)
        {
            var versionString = YohkanUtil.CreateRemoteCatalogVersionString(remoteCatalogSuffix);
            var setting = YohkanUtil.GetSettings();
            setting.OverridePlayerVersion = versionString;
            setting.ContentStateBuildPath = YohkanUtil.CreateContentStateBuildPathString(remoteCatalogSuffix);
            setting.BuildRemoteCatalog = true;
            //update remote build & load path.
            var profile = setting.profileSettings;
            var activeProfileId = setting.activeProfileId;
            var remoteLoadVariableData = profile.GetProfileDataByName("Remote.LoadPath");
            var remoteBuildVariableData = profile.GetProfileDataByName("Remote.BuildPath");
            profile.SetValue(activeProfileId,remoteLoadVariableData.ProfileName,YohkanUtil.CreateRemoteLoadPath(remoteCatalogSuffix));
            profile.SetValue(activeProfileId,remoteBuildVariableData.ProfileName,YohkanUtil.CreateRemoteBuildPath(remoteCatalogSuffix));
        }


        private static void PrepareAssetBuildProcess(Action<AddressableAssetSettings> onPreProcess)
        {
            var config = YohkanUtil.GetBuilderConfig();
            var settings = YohkanUtil.GetSettings();
          
            
            if (config.EnableYohkanAddressableLabelSetWhenBundleBuild)
            {
                //set yohkan_all_dl label.
                foreach (var group in settings.groups)
                {
                    var updateSchema = group.GetSchema<ContentUpdateGroupSchema>();
                    if (updateSchema != null && updateSchema.StaticContent) continue;
                    
                    foreach (var entry in group.entries)
                    {
                        if (!entry.labels.Contains(YohkanEditorConst.ALL_DOWNLOAD_LABEL))
                        {
                            entry.SetLabel(YohkanEditorConst.ALL_DOWNLOAD_LABEL, true, true);
                        }
                    }
                }
            }

            onPreProcess?.Invoke(settings);
        }
    }
}
