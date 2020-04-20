#if UNITY_2020_1_OR_NEWER
using UnityEngine;
#endif

namespace Unity.Build
{
    /// <summary>
    /// Base interface for build configuration components that provides a build pipeline.
    /// </summary>
    public interface IBuildPipelineComponent : IBuildComponent
    {
        /// <summary>
        /// Build pipeline used by this build configuration.
        /// </summary>
#if UNITY_2020_1_OR_NEWER
        LazyLoadReference<BuildPipeline> Pipeline { get; set; }
#else
        BuildPipeline Pipeline { get; set; }
#endif

        /// <summary>
        /// Returns index which is used for sorting builds when they're batch in build queue
        /// </summary>
        int SortingIndex { get; }

        /// <summary>
        /// Sets the editor environment before starting the build
        /// </summary>
        /// <returns>Returns true, when editor domain reload is required.</returns>
        bool SetupEnvironment();
    }
}
