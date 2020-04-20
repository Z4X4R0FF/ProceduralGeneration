using System;

namespace Unity.Build
{
    /// <summary>
    /// Can stores a set of hierarchical build components per type, which can be inherited or overridden using dependencies.
    /// </summary>
    public sealed class BuildConfiguration : HierarchicalComponentContainer<BuildConfiguration, IBuildComponent>
    {
        /// <summary>
        /// File extension for build configuration assets.
        /// </summary>
        public const string AssetExtension = ".buildconfiguration";

        /// <summary>
        /// Retrieve the build pipeline of this build configuration.
        /// </summary>
        /// <returns>The build pipeline if found, otherwise <see langword="null"/>.</returns>
        public BuildPipeline GetBuildPipeline()
        {
            if (TryGetComponent<IBuildPipelineComponent>(out var component))
            {
#if UNITY_2020_1_OR_NEWER
                var pipeline = component.Pipeline.asset;
#else
                var pipeline = component.Pipeline;
#endif
                return pipeline != null && pipeline ? pipeline : null;
            }
            return null;
        }

        /// <summary>
        /// Determine if the build pipeline of this build configuration can build.
        /// </summary>
        /// <param name="reason">If <see cref="CanBuild"/> returns <see langword="false"/>, the reason why it fails.</param>
        /// <returns>Whether or not the build pipeline can build.</returns>
        public bool CanBuild(out string reason)
        {
            var pipeline = GetBuildPipeline();
            if (pipeline == null)
            {
                reason = $"No valid build pipeline found for {name.ToHyperLink()}. At least one component that derives from {nameof(IBuildPipelineComponent)} must be present.";
                return false;
            }
            return pipeline.CanBuild(this, out reason);
        }

        /// <summary>
        /// Run the build pipeline of this build configuration to build the target.
        /// </summary>
        /// <returns>The result of the build pipeline build.</returns>
        public BuildPipelineResult Build()
        {
            var pipeline = GetBuildPipeline();
            if (!CanBuild(out var reason))
            {
                return BuildPipelineResult.Failure(pipeline, this, reason);
            }

            var what = !string.IsNullOrEmpty(name) ? $" {name}" : string.Empty;
            using (var progress = new BuildProgress($"Building{what}", "Please wait..."))
            {
                return pipeline.Build(this, progress);
            }
        }

        /// <summary>
        /// Determine if the build pipeline of this build configuration can run.
        /// </summary>
        /// <param name="reason">If <see cref="CanRun"/> returns <see langword="false"/>, the reason why it fails.</param>
        /// <returns>Whether or not the build pipeline can run.</returns>
        public bool CanRun(out string reason)
        {
            var pipeline = GetBuildPipeline();
            if (pipeline == null)
            {
                reason = $"No valid build pipeline found for {name.ToHyperLink()}. At least one component that derives from {nameof(IBuildPipelineComponent)} must be present.";
                return false;
            }
            return pipeline.CanRun(this, out reason);
        }

        /// <summary>
        /// Run the resulting target from building the build pipeline of this build configuration.
        /// </summary>
        /// <returns></returns>
        public RunStepResult Run()
        {
            var pipeline = GetBuildPipeline();
            if (!CanRun(out var reason))
            {
                return RunStepResult.Failure(this, pipeline?.RunStep, reason);
            }
            return pipeline.Run(this);
        }

        /// <summary>
        /// Get the value of the first build artifact that is assignable to type <see cref="Type"/>.
        /// </summary>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <param name="type">The type of the build artifact.</param>
        /// <returns>The build artifact if found, otherwise <see langword="null"/>.</returns>
        public IBuildArtifact GetLastBuildArtifact(Type type) => BuildArtifacts.GetBuildArtifact(this, type);

        /// <summary>
        /// Get the value of the first build artifact that is assignable to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the build artifact.</typeparam>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <returns>The build artifact if found, otherwise <see langword="null"/>.</returns>
        public T GetLastBuildArtifact<T>() where T : class, IBuildArtifact => BuildArtifacts.GetBuildArtifact<T>(this);

        /// <summary>
        /// Get the last build result for this build configuration.
        /// </summary>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <returns>The build result if found, otherwise <see langword="null"/>.</returns>
        public BuildPipelineResult GetLastBuildResult() => BuildArtifacts.GetBuildResult(this);
    }
}
