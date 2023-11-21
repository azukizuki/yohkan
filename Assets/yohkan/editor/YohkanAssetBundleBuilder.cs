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

        public static void BuildAssetBundle(string remoteCatalogSuffix)
        {
            SetUpAddressablePaths(remoteCatalogSuffix);
            PrepareAssetBuildProcess();
            AddressableAssetSettings.BuildPlayerContent();
        }

        public static void BuildWithContentState(string remoteCatalogSuffix)
        {
            SetUpAddressablePaths(remoteCatalogSuffix);
            PrepareAssetBuildProcess();

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
            
            profile.SetValue(activeProfileId,setting.RemoteCatalogBuildPath.GetName(setting),YohkanUtil.CreateRemoteBuildPath(remoteCatalogSuffix));
            profile.SetValue(activeProfileId,setting.RemoteCatalogLoadPath.GetName(setting),YohkanUtil.CreateRemoteLoadPath(remoteCatalogSuffix));
        }


        private static void PrepareAssetBuildProcess()
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
        }
    }
}
