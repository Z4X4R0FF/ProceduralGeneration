using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.Properties;
using UnityEngine;

namespace Unity.Build.Tests
{
    class BuildTestsBase
    {
        [HideInInspector]
        protected class TestPipelineComponent : IBuildPipelineComponent
        {
#if UNITY_2020_1_OR_NEWER
            [CreateProperty] public LazyLoadReference<BuildPipeline> Pipeline { get; set; }
#else
            [CreateProperty] public BuildPipeline Pipeline { get; set; }
#endif
            public int SortingIndex => 0;
            public bool SetupEnvironment() => false;
        }

        [HideInInspector]
        protected class TestRequiredComponentA : IBuildComponent { }

        [HideInInspector]
        protected class TestRequiredComponentB : IBuildComponent { }

        [HideInInspector]
        protected class TestOptionalComponentA : IBuildComponent { }

        [HideInInspector]
        protected class TestOptionalComponentB : IBuildComponent { }

        protected class TestInvalidComponent { }

        [HideInInspector, BuildStep(Name = "this is a name", Description = "this is a description", Category = "this is a category")]
        protected class TestBuildStep : BuildStep
        {
            public override BuildStepResult RunBuildStep(BuildContext context) => Success();
        }

        [HideInInspector]
        protected class TestBuildStepFailure : BuildStep
        {
            public override BuildStepResult RunBuildStep(BuildContext context) => Failure(nameof(TestBuildStepFailure));
        }

        [HideInInspector]
        protected class TestBuildStepThrows : BuildStep
        {
            public override BuildStepResult RunBuildStep(BuildContext context) => throw new InvalidOperationException();
        }

        [HideInInspector]
        protected class TestBuildStepWithRequirements : BuildStep
        {
            public override Type[] RequiredComponents => new[] { typeof(TestRequiredComponentA), typeof(TestRequiredComponentB) };
            public override Type[] OptionalComponents => new[] { typeof(TestOptionalComponentA), typeof(TestOptionalComponentB) };
            public override BuildStepResult RunBuildStep(BuildContext context) => throw new NotImplementedException();
        }

        protected class TestInvalidBuildStep { }

        [HideInInspector]
        [BuildStepRunBefore(typeof(TestBuildStep2))]
        protected class TestBuildStep1 : BuildStep
        {
            readonly bool m_IsEnabled;

            public class Data
            {
                public string Value;
            }

            public TestBuildStep1(bool enabled = true)
            {
                m_IsEnabled = enabled;
            }

            public override Type[] RequiredComponents => new[] { typeof(TestRequiredComponentA) };

            public override Type[] OptionalComponents => new[] { typeof(TestOptionalComponentA) };

            public override bool IsEnabled(BuildContext context) => m_IsEnabled;

            public override BuildStepResult RunBuildStep(BuildContext context)
            {
                context.GetOrCreateValue<TestArtifactA>().BuildStepsRan.Add(nameof(TestBuildStep1));
                return context.GetValue<Data>().Value == nameof(TestBuildStep1) ? Success() : Failure(nameof(TestBuildStep1));
            }

            public override BuildStepResult CleanupBuildStep(BuildContext context)
            {
                context.GetOrCreateValue<TestArtifactA>().CleanupStepsRan.Add(nameof(TestBuildStep1));
                return context.GetValue<Data>().Value == nameof(TestBuildStep1) ? Success() : Failure(nameof(TestBuildStep1));
            }
        }

        [HideInInspector]
        [BuildStepRunAfter(typeof(TestBuildStep1))]
        protected class TestBuildStep2 : BuildStep
        {
            readonly bool m_IsEnabled;

            public class Data
            {
                public string Value;
            }

            public TestBuildStep2(bool enabled = true)
            {
                m_IsEnabled = enabled;
            }

            public override Type[] RequiredComponents => new[] { typeof(TestRequiredComponentA) };

            public override Type[] OptionalComponents => new[] { typeof(TestOptionalComponentA) };

            public override bool IsEnabled(BuildContext context) => m_IsEnabled;

            public override BuildStepResult RunBuildStep(BuildContext context)
            {
                context.GetOrCreateValue<TestArtifactA>().BuildStepsRan.Add(nameof(TestBuildStep2));
                return context.GetValue<Data>().Value == nameof(TestBuildStep2) ? Success() : Failure(nameof(TestBuildStep2));
            }

            public override BuildStepResult CleanupBuildStep(BuildContext context)
            {
                context.GetOrCreateValue<TestArtifactA>().CleanupStepsRan.Add(nameof(TestBuildStep2));
                return context.GetValue<Data>().Value == nameof(TestBuildStep2) ? Success() : Failure(nameof(TestBuildStep2));
            }
        }

        [HideInInspector]
        [BuildStepRunAfter(typeof(TestBuildStep2))]
        protected class TestBuildStep3 : BuildStep
        {
            readonly bool m_IsEnabled;

            public class Data
            {
                public string Value;
            }

            public TestBuildStep3(bool enabled = true)
            {
                m_IsEnabled = enabled;
            }

            public override bool IsEnabled(BuildContext context) => m_IsEnabled;

            public override Type[] RequiredComponents => new[] { typeof(TestRequiredComponentA) };

            public override Type[] OptionalComponents => new[] { typeof(TestOptionalComponentA) };

            public override BuildStepResult RunBuildStep(BuildContext context)
            {
                context.GetOrCreateValue<TestArtifactA>().BuildStepsRan.Add(nameof(TestBuildStep3));
                return context.GetValue<Data>().Value == nameof(TestBuildStep3) ? Success() : Failure(nameof(TestBuildStep3));
            }

            public override BuildStepResult CleanupBuildStep(BuildContext context)
            {
                context.GetOrCreateValue<TestArtifactA>().CleanupStepsRan.Add(nameof(TestBuildStep3));
                return context.GetValue<Data>().Value == nameof(TestBuildStep3) ? Success() : Failure(nameof(TestBuildStep3));
            }
        }

        [HideInInspector]
        protected class TestBuildStepWithInvalidRequiredComponent : BuildStep
        {
            public override Type[] RequiredComponents => new[] { typeof(TestInvalidComponent) };
            public override BuildStepResult RunBuildStep(BuildContext context) => Success();
        }

        [HideInInspector]
        protected class TestBuildStepWithInvalidOptionalComponent : BuildStep
        {
            public override Type[] OptionalComponents => new[] { typeof(TestInvalidComponent) };
            public override BuildStepResult RunBuildStep(BuildContext context) => Success();
        }

        protected class TestRunInstance : IRunInstance
        {
            public TestRunInstance()
            {
                IsRunning = true;
            }

            public bool IsRunning { get; private set; }
            public void Dispose() { IsRunning = false; }
        }

        [HideInInspector]
        protected class TestRunStep : RunStep
        {
            public override RunStepResult Start(BuildConfiguration config) => Success(config, new TestRunInstance());
        }

        [HideInInspector]
        protected class TestRunStepFailure : RunStep
        {
            public override RunStepResult Start(BuildConfiguration config) => Failure(config, nameof(TestRunStepFailure));
        }

        [HideInInspector]
        protected class TestRunStepCannotRun : RunStep
        {
            public override bool CanRun(BuildConfiguration config, out string reason)
            {
                reason = nameof(TestRunStepCannotRun);
                return false;
            }

            public override RunStepResult Start(BuildConfiguration config) => Success(config, new TestRunInstance());
        }

        [HideInInspector]
        protected class TestRunStepThrows : RunStep
        {
            public override RunStepResult Start(BuildConfiguration config) => throw new InvalidOperationException();
        }

        protected class TestArtifactA : IBuildArtifact
        {
            public List<string> BuildStepsRan = new List<string>();
            public List<string> CleanupStepsRan = new List<string>();
        }

        protected class TestArtifactB : IBuildArtifact { }

        protected class TestInvalidArtifact { }

        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(BuildArtifacts.BaseDirectory))
            {
                Directory.Delete(BuildArtifacts.BaseDirectory, true);
            }
            BuildArtifacts.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(BuildArtifacts.BaseDirectory))
            {
                Directory.Delete(BuildArtifacts.BaseDirectory, true);
            }
            BuildArtifacts.Clear();
        }

        protected static IEnumerable<Type> FindAllDerivedTypes<T>()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in FindAllDerivedTypes<T>(assembly))
                {
                    yield return type;
                }
            }
        }

        protected static IEnumerable<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (type == derivedType)
                {
                    continue;
                }

                if (!derivedType.IsAssignableFrom(type))
                {
                    continue;
                }

                yield return type;
            }
        }
    }
}
