using UnityEditor.Experimental.AssetImporters;

namespace Unity.Build
{
#if UNITY_2020_1_OR_NEWER
    [ScriptedImporter(3, new[] { BuildPipeline.AssetExtension })]
#else
    [ScriptedImporter(2, new[] { BuildPipeline.AssetExtension })]
#endif
    sealed class BuildPipelineScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            var asset = BuildPipeline.CreateInstance();
            if (BuildPipeline.DeserializeFromPath(asset, context.assetPath))
            {
                context.AddObjectToAsset("asset", asset/*, icon*/);
                context.SetMainObject(asset);
            }
        }
    }
}
