using System;
using Unity.Properties;

namespace Unity.Build
{
    /// <summary>
    /// Holds the result of the execution of a <see cref="Build.BuildStep"/>.
    /// </summary>
    public sealed class BuildStepResult : ResultBase
    {
        /// <summary>
        /// The <see cref="Build.BuildStep"/> that was executed.
        /// </summary>
        [CreateProperty] public BuildStep BuildStep { get; internal set; }

        /// <summary>
        /// Description of the <see cref="Build.BuildStep"/>.
        /// </summary>
        [CreateProperty] public string Description => BuildStep.Description;

        /// <summary>
        /// Implicit conversion to <see cref="bool"/>.
        /// </summary>
        /// <param name="result">Instance of <see cref="BuildStepResult"/>.</param>
        public static implicit operator bool(BuildStepResult result) => result.Succeeded;

        /// <summary>
        /// Create a new instance of <see cref="BuildStepResult"/> from a <see cref="UnityEditor.Build.Reporting.BuildReport"/>.
        /// </summary>
        /// <param name="step">The <see cref="Build.BuildStep"/> that was executed.</param>
        /// <param name="report">The report that was generated.</param>
        public BuildStepResult(BuildStep step, UnityEditor.Build.Reporting.BuildReport report)
        {
            Succeeded = report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
            Message = Failed ? report.summary.ToString() : null;
            BuildStep = step;
        }

        /// <summary>
        /// Create a new instance of <see cref="BuildStepResult"/> that represent a successful execution.
        /// </summary>
        /// <param name="step">The <see cref="Build.BuildStep"/> that was executed.</param>
        /// <returns>A new <see cref="BuildStepResult"/> instance.</returns>
        public static BuildStepResult Success(BuildStep step) => new BuildStepResult
        {
            Succeeded = true,
            BuildStep = step
        };

        /// <summary>
        /// Create a new instance of <see cref="BuildStepResult"/> that represent a failed execution.
        /// </summary>
        /// <param name="step">The <see cref="Build.BuildStep"/> that was executed.</param>
        /// <param name="message">The failure message.</param>
        /// <returns>A new <see cref="BuildStepResult"/> instance.</returns>
        public static BuildStepResult Failure(BuildStep step, string message) => new BuildStepResult
        {
            Succeeded = false,
            Message = message,
            BuildStep = step
        };

        /// <summary>
        /// Create a new instance of <see cref="BuildStepResult"/> that represent a failed execution.
        /// </summary>
        /// <param name="step">The <see cref="Build.BuildStep"/> that was executed.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>A new <see cref="BuildStepResult"/> instance.</returns>
        public static BuildStepResult Failure(BuildStep step, Exception exception) => new BuildStepResult
        {
            Succeeded = false,
            BuildStep = step,
            Exception = exception
        };

        public override string ToString() => $"Build {base.ToString()}";

        public BuildStepResult() { }
    }
}
