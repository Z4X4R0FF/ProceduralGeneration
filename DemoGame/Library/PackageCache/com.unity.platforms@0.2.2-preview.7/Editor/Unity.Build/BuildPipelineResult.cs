using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Properties;

namespace Unity.Build
{
    /// <summary>
    /// Holds the results of the execution of a <see cref="Build.BuildPipeline"/>.
    /// </summary>
    public sealed class BuildPipelineResult : ResultBase
    {
        /// <summary>
        /// A list of <see cref="BuildStepResult"/> collected during the <see cref="Build.BuildPipeline"/> execution for each <see cref="IBuildStep"/>.
        /// </summary>
        [CreateProperty] public List<BuildStepResult> BuildStepsResults { get; internal set; } = new List<BuildStepResult>();

        /// <summary>
        /// Get the <see cref="BuildStepResult"/> for the specified <see cref="IBuildStep"/>.
        /// </summary>
        /// <param name="buildStep">The build step to search for the result.</param>
        /// <param name="value">The <see cref="BuildStepResult"/> if found, otherwise default(<see cref="BuildStepResult"/>)</param>
        /// <returns><see langword="true"/> if the IBuildStep was found, otherwise <see langword="false"/>.</returns>
        public bool TryGetBuildStepResult(IBuildStep buildStep, out BuildStepResult value)
        {
            foreach (var result in BuildStepsResults)
            {
                if (result.BuildStep == buildStep)
                {
                    value = result;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Implicit conversion to <see cref="bool"/>.
        /// </summary>
        /// <param name="result">Instance of <see cref="BuildPipelineResult"/>.</param>
        public static implicit operator bool(BuildPipelineResult result) => result.Succeeded;

        /// <summary>
        /// Create a new instance of <see cref="BuildPipelineResult"/> that represent a successful execution.
        /// </summary>
        /// <param name="config">The <see cref="Build.BuildConfiguration"/> used throughout this <see cref="Build.BuildPipeline"/> execution.</param>
        /// <returns>A new <see cref="BuildPipelineResult"/> instance.</returns>
        public static BuildPipelineResult Success(BuildPipeline pipeline, BuildConfiguration config) => new BuildPipelineResult
        {
            Succeeded = true,
            BuildPipeline = pipeline,
            BuildConfiguration = config
        };

        /// <summary>
        /// Create a new instance of <see cref="BuildPipelineResult"/> that represent a failed execution.
        /// </summary>
        /// <param name="config">The <see cref="Build.BuildConfiguration"/> used throughout this <see cref="Build.BuildPipeline"/> execution.</param>
        /// <param name="message">The failure message.</param>
        /// <returns>A new <see cref="BuildPipelineResult"/> instance.</returns>
        public static BuildPipelineResult Failure(BuildPipeline pipeline, BuildConfiguration config, string message) => new BuildPipelineResult
        {
            Succeeded = false,
            BuildPipeline = pipeline,
            BuildConfiguration = config,
            Message = message
        };

        /// <summary>
        /// Create a new instance of <see cref="BuildPipelineResult"/> that represent a failed execution.
        /// </summary>
        /// <param name="config">The <see cref="Build.BuildConfiguration"/> used throughout this <see cref="Build.BuildPipeline"/> execution.</param>
        /// <param name="message">The failure message.</param>
        /// <returns>A new <see cref="BuildPipelineResult"/> instance.</returns>
        public static BuildPipelineResult Failure(BuildPipeline pipeline, BuildConfiguration config, Exception exception) => new BuildPipelineResult
        {
            Succeeded = false,
            BuildPipeline = pipeline,
            BuildConfiguration = config,
            Exception = exception
        };

        public override string ToString()
        {
            var name = BuildConfiguration.name;
            var what = !string.IsNullOrEmpty(name) ? $" {name.ToHyperLink()}" : string.Empty;
            if (Succeeded)
            {
                return $"Build{what} succeeded after {Duration.ToShortString()}.";
            }
            else
            {
                var result = BuildStepsResults.FirstOrDefault(r => r.Failed);
                if (result != null && result.Failed)
                {
                    return $"Build{what} failed in step '{result.BuildStep.Name}' after {Duration.ToShortString()}.\n{Message}";
                }
                else
                {
                    return $"Build{what} failed after {Duration.ToShortString()}.\n{Message}";
                }
            }
        }

        public BuildPipelineResult() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("BuildSettings has been renamed to BuildConfiguration. (RemovedAfter 2020-05-01) (UnityUpgradable) -> BuildConfiguration")]
        public BuildConfiguration BuildSettings { get; internal set; }
    }
}
