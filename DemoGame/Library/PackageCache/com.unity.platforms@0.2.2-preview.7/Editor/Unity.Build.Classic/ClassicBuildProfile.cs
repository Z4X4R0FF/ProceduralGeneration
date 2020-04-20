using System;
using System.Collections.Generic;
using Unity.Properties;
using Unity.Serialization;
using UnityEditor;
using UnityEngine;

namespace Unity.Build.Classic
{
    [FormerName("Unity.Build.Common.ClassicBuildProfile, Unity.Build.Common")]
    public sealed class ClassicBuildProfile : IBuildPipelineComponent
    {
        BuildTarget m_Target;
        List<string> m_ExcludedAssemblies;

        /// <summary>
        /// Retrieve <see cref="BuildTypeCache"/> for this build profile.
        /// </summary>
        public BuildTypeCache TypeCache { get; } = new BuildTypeCache();

        /// <summary>
        /// Gets or sets which <see cref="BuildTarget"/> this profile is going to use for the build.
        /// Used for building classic Unity standalone players.
        /// </summary>
        [CreateProperty]
        public BuildTarget Target
        {
            get => m_Target;
            set
            {
                m_Target = value;
                TypeCache.PlatformName = m_Target.ToString();
            }
        }

        /// <summary>
        /// Gets or sets which <see cref="BuildType"/> this profile is going to use for the build.
        /// </summary>
        [CreateProperty]
        public BuildType Configuration { get; set; } = BuildType.Develop;

#if UNITY_2020_1_OR_NEWER
        [CreateProperty] public LazyLoadReference<BuildPipeline> Pipeline { get; set; }
#else
        [CreateProperty] public BuildPipeline Pipeline { get; set; }
#endif

        public int SortingIndex => (int)m_Target;

        public bool SetupEnvironment()
        {
            if (m_Target == EditorUserBuildSettings.activeBuildTarget)
                return false;
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(m_Target), m_Target))
                throw new Exception($"Failed to switch active build target to {m_Target}");
            return true;
        }

        /// <summary>
        /// List of assemblies that should be explicitly excluded for the build.
        /// </summary>
        [CreateProperty, HideInInspector]
        public List<string> ExcludedAssemblies
        {
            get => m_ExcludedAssemblies;
            set
            {
                m_ExcludedAssemblies = value;
                TypeCache.ExcludedAssemblies = value;
            }
        }

        public ClassicBuildProfile()
        {
            Target = BuildTarget.NoTarget;
            ExcludedAssemblies = new List<string>();
        }

        /// <summary>
        /// Gets the extension string for the target platform.
        /// </summary>
        /// <returns>The extension string.</returns>
        public string GetExecutableExtension()
        {
#pragma warning disable CS0618
            switch (m_Target)
            {
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                    return ".app";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return ".exe";
                case BuildTarget.NoTarget:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.Stadia:
                    return string.Empty;
                case BuildTarget.Android:
                    if (EditorUserBuildSettings.exportAsGoogleAndroidProject)
                        return "";
                    else if (EditorUserBuildSettings.buildAppBundle)
                        return ".aab";
                    else
                        return ".apk";
                case BuildTarget.Lumin:
                    return ".mpk";
                case BuildTarget.iOS:
                case BuildTarget.tvOS:
                default:
                    return "";
            }
#pragma warning restore CS0618
        }
    }
}
