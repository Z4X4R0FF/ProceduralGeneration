using System;
using System.IO;
using Unity.Build.Common;
using System.Collections.Generic;
using Unity.Serialization;
using UnityEditor;

namespace Unity.Build.Classic
{
    internal class ScenePathsState : List<string>
    {
        public ScenePathsState()
            : base() { }

        public ScenePathsState(int capacity)
            : base(capacity) { }

        public ScenePathsState(IEnumerable<string> collection)
            : base(collection) { }
    }

    internal class BuildOptionsState
    {
        public BuildOptionsState()
        {
            BuildOptions = BuildOptions.None;
        }

        public BuildOptions BuildOptions;
    }

    [BuildStep(Name = "Build Player", Description = "Building Player", Category = "Classic")]
    [FormerName("Unity.Build.Common.BuildStepBuildClassicPlayer, Unity.Build.Common")]
    sealed class BuildStepBuildClassicPlayer : BuildStep
    {
        public override Type[] RequiredComponents => new[]
        {
            typeof(ClassicBuildProfile),
            typeof(SceneList),
            typeof(GeneralSettings)
        };

        public override Type[] OptionalComponents => new[]
        {
            typeof(OutputBuildDirectory),
            typeof(SourceBuildConfiguration),
            typeof(TestablePlayer)
        };

        private bool UseAutoRunPlayer(BuildContext context)
        {
#if UNITY_2020_1_OR_NEWER
            var pipeline = GetRequiredComponent<ClassicBuildProfile>(context).Pipeline.asset;
#else
            var pipeline = GetRequiredComponent<ClassicBuildProfile>(context).Pipeline;
#endif
            var runStep = pipeline.RunStep;

            // RunStep is provided no need to use AutoRunPlayer
            if (runStep != null && runStep.GetType() != typeof(RunStepNotImplemented))
                return false;

            // See dots\Samples\Library\PackageCache\com.unity.build@0.1.0-preview.1\Editor\Unity.Build\BuildSettingsScriptedImporterEditor.cs
            const string k_CurrentActionKey = "BuildAction-CurrentAction";
            if (!EditorPrefs.HasKey(k_CurrentActionKey))
                return false;

            var value = EditorPrefs.GetInt(k_CurrentActionKey);
            return value == 1;
        }

        public override BuildStepResult RunBuildStep(BuildContext context)
        {
            BuildPlayerOptions buildPlayerOptions = default;
            var generalSettings = GetRequiredComponent<GeneralSettings>(context);
            var profile = GetRequiredComponent<ClassicBuildProfile>(context);
            if (profile.Target <= 0)
                return BuildStepResult.Failure(this, $"Invalid build target '{profile.Target.ToString()}'.");

            if (profile.Target != EditorUserBuildSettings.activeBuildTarget)
                return BuildStepResult.Failure(this, $"{nameof(EditorUserBuildSettings.activeBuildTarget)} must be switched before {nameof(BuildStepBuildClassicPlayer)} step.");

            // Check for incoming scene list from build context
            string[] scenePaths = default;
            if (context.HasValue<ScenePathsState>())
            {
                scenePaths = context.GetValue<ScenePathsState>().ToArray();
            }
            else
            {
                var sceneList = GetRequiredComponent<SceneList>(context);
                scenePaths = sceneList.GetScenePathsForBuild();
            }

            if(scenePaths.Length == 0)
                return BuildStepResult.Failure(this, "There are no scenes to build.");

            var outputPath = this.GetOutputBuildDirectory(context);
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = scenePaths,
                target = profile.Target,
                locationPathName = Path.Combine(outputPath, generalSettings.ProductName + profile.GetExecutableExtension()),
                targetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(profile.Target),
            };

            buildPlayerOptions.options = context.GetOrCreateValue<BuildOptionsState>().BuildOptions;

            switch (profile.Configuration)
            {
                case BuildType.Debug:
                    buildPlayerOptions.options |= BuildOptions.AllowDebugging | BuildOptions.Development;
                    break;
                case BuildType.Develop:
                    buildPlayerOptions.options |= BuildOptions.Development;
                    break;
            }

            var sourceBuild = GetOptionalComponent<SourceBuildConfiguration>(context);
            if (sourceBuild.Enabled)
            {
                buildPlayerOptions.options |= BuildOptions.InstallInBuildFolder;
            }

            if (HasOptionalComponent<TestablePlayer>(context))
                buildPlayerOptions.options |= BuildOptions.IncludeTestAssemblies | BuildOptions.ConnectToHost;

            if (UseAutoRunPlayer(context))
            {
                UnityEngine.Debug.Log($"Using BuildOptions.AutoRunPlayer, since RunStep is not provided for {profile.Target}");
                buildPlayerOptions.options |= BuildOptions.AutoRunPlayer;
            }

            var report = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);
            var result = new BuildStepResult(this, report);
            context.SetValue(report);

            return result;
        }

        public override BuildStepResult CleanupBuildStep(BuildContext context)
        {
            return Success();
        }
    }
}
