using System;

namespace Unity.Build
{
    /// <summary>
    /// Holds contextual information when building a build pipeline.
    /// </summary>
    public sealed class BuildContext : ContextBase
    {
        /// <summary>
        /// Current build pipeline execution status.
        /// </summary>
        public BuildPipelineResult BuildPipelineStatus { get; }

        /// <summary>
        /// The build progress object used througout the build.
        /// </summary>
        public BuildProgress BuildProgress { get; }

        /// <summary>
        /// Quick access to build manifest value.
        /// </summary>
        public BuildManifest BuildManifest => GetOrCreateValue<BuildManifest>();

        /// <summary>
        /// Get a build result representing a success.
        /// </summary>
        /// <returns>A new build result instance.</returns>
        public BuildPipelineResult Success() => BuildPipelineResult.Success(BuildPipeline, BuildConfiguration);

        /// <summary>
        /// Get a build result representing a failure.
        /// </summary>
        /// <param name="reason">The reason of the failure.</param>
        /// <returns>A new build result instance.</returns>
        public BuildPipelineResult Failure(string reason) => BuildPipelineResult.Failure(BuildPipeline, BuildConfiguration, reason);

        /// <summary>
        /// Get a build result representing a failure.
        /// </summary>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A new build result instance.</returns>
        public BuildPipelineResult Failure(Exception exception) => BuildPipelineResult.Failure(BuildPipeline, BuildConfiguration, exception);

        internal BuildContext() { }

        internal BuildContext(BuildPipeline pipeline, BuildConfiguration config, BuildProgress progress = null, Action<BuildContext> mutator = null) :
            base(pipeline, config)
        {
            BuildPipelineStatus = BuildPipelineResult.Success(pipeline, config);
            BuildProgress = progress;
            mutator?.Invoke(this);
        }
    }
}
