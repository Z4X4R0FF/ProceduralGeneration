using Bee.NativeProgramSupport.Building;
using System.Collections.Generic;
using Unity.BuildSystem.CSharpSupport;
using Unity.BuildSystem.NativeProgramSupport;

namespace DotsBuildTargets
{
    abstract class DotsBuildSystemTarget
    {
        public virtual IEnumerable<DotsRuntimeCSharpProgramConfiguration> GetConfigs()
        {
            if (!ToolChain.CanBuild)
                yield break;

            yield return new DotsRuntimeCSharpProgramConfiguration(
                csharpCodegen: CSharpCodeGen.Debug,
                cppCodegen: CodeGen.Debug,
                nativeToolchain: ToolChain,
                scriptingBackend: ScriptingBackend,
                targetFramework: TargetFramework,
                identifier: $"{Identifier}-debug",
                enableUnityCollectionsChecks: true,
                enableManagedDebugging: false,
                waitForManagedDebugger: true,
                multiThreadedJobs: CanRunMultiThreadedJobs,
                dotsConfiguration: DotsConfiguration.Debug,
                useBurst: CanUseBurst,
                executableFormat: GetExecutableFormatForConfig(DotsConfiguration.Debug, enableManagedDebugger: false));

            yield return new DotsRuntimeCSharpProgramConfiguration(
                csharpCodegen: CSharpCodeGen.Debug,
                cppCodegen: CodeGen.Release,
                nativeToolchain: ToolChain,
                scriptingBackend: ScriptingBackend,
                targetFramework: TargetFramework,
                identifier: $"{Identifier}-mdb",
                enableUnityCollectionsChecks: true,
                enableManagedDebugging: true,
                waitForManagedDebugger: true,
                multiThreadedJobs: CanRunMultiThreadedJobs,
                dotsConfiguration: DotsConfiguration.Debug,
                useBurst: CanUseBurst,
                executableFormat: GetExecutableFormatForConfig(DotsConfiguration.Debug, enableManagedDebugger: true));

            yield return new DotsRuntimeCSharpProgramConfiguration(
                csharpCodegen: CSharpCodeGen.Release,
                cppCodegen: CodeGen.Release,
                nativeToolchain: ToolChain,
                scriptingBackend: ScriptingBackend,
                targetFramework: TargetFramework,
                identifier: $"{Identifier}-develop",
                enableUnityCollectionsChecks: true,
                enableManagedDebugging: false,
                waitForManagedDebugger: true,
                multiThreadedJobs: CanRunMultiThreadedJobs,
                dotsConfiguration: DotsConfiguration.Develop,
                useBurst: CanUseBurst,
                executableFormat: GetExecutableFormatForConfig(DotsConfiguration.Develop, enableManagedDebugger: false));

            yield return new DotsRuntimeCSharpProgramConfiguration(
                csharpCodegen: CSharpCodeGen.Release,
                cppCodegen: CodeGen.Release,
                nativeToolchain: ToolChain,
                scriptingBackend: ScriptingBackend,
                targetFramework: TargetFramework,
                identifier: $"{Identifier}-release",
                enableUnityCollectionsChecks: false,
                enableManagedDebugging: false,
                waitForManagedDebugger: true,
                multiThreadedJobs: CanRunMultiThreadedJobs,
                dotsConfiguration: DotsConfiguration.Release,
                useBurst: CanUseBurst,
                executableFormat: GetExecutableFormatForConfig(DotsConfiguration.Release, enableManagedDebugger: false));
        }

        protected virtual bool CanRunMultiThreadedJobs => false; // Disabling by default; Eventually: ScriptingBackend == ScriptingBackend.Dotnet;
        /*
         * disabled by default because it takes work to enable each platform for burst
         */
        public virtual bool CanUseBurst => false;

        public abstract string Identifier { get; }
        public abstract ToolChain ToolChain { get; }

        public virtual ScriptingBackend ScriptingBackend { get; set; } = ScriptingBackend.TinyIl2cpp;
        public virtual TargetFramework TargetFramework => TargetFramework.Tiny;

        protected virtual NativeProgramFormat GetExecutableFormatForConfig(DotsConfiguration config, bool enableManagedDebugger) => null;

        public virtual NativeProgramFormat CustomizeExecutableForSettings(FriendlyJObject settings) => null;

    }
}
