using System;
using System.ComponentModel;

namespace Unity.Build
{
    /// <summary>
    /// Holds the result of the execution of a <see cref="Build.RunStep"/>.
    /// </summary>
    public sealed class RunStepResult : ResultBase, IDisposable
    {
        /// <summary>
        /// The <see cref="Build.RunStep"/> that was executed.
        /// </summary>
        public RunStep RunStep { get; internal set; }

        /// <summary>
        /// The running process resulting from running the <see cref="Build.RunStep"/>.
        /// </summary>
        public IRunInstance RunInstance { get; internal set; }

        /// <summary>
        /// Implicit conversion to <see cref="bool"/>.
        /// </summary>
        /// <param name="result">Instance of <see cref="RunStepResult"/>.</param>
        public static implicit operator bool(RunStepResult result) => result.Succeeded;

        /// <summary>
        /// Create a new instance of <see cref="RunStepResult"/> that represent a successful execution.
        /// </summary>
        /// <param name="config">The <see cref="BuildConfiguration"/> used by the <see cref="Build.RunStep"/>.</param>
        /// <param name="step">The <see cref="Build.RunStep"/> that was executed.</param>
        /// <param name="instance">The <see cref="IRunInstance"/> resulting from running this <see cref="Build.RunStep"/>.</param>
        /// <returns>A new <see cref="RunStepResult"/> instance.</returns>
        public static RunStepResult Success(BuildConfiguration config, RunStep step, IRunInstance instance) => new RunStepResult
        {
            Succeeded = true,
            BuildConfiguration = config,
            RunStep = step,
            RunInstance = instance
        };

        /// <summary>
        /// Create a new instance of <see cref="RunStepResult"/> that represent a failed execution.
        /// </summary>
        /// <param name="config">The <see cref="BuildConfiguration"/> used by the <see cref="Build.RunStep"/>.</param>
        /// <param name="step">The <see cref="Build.RunStep"/> that was executed.</param>
        /// <param name="message">The failure message.</param>
        /// <returns>A new <see cref="RunStepResult"/> instance.</returns>
        public static RunStepResult Failure(BuildConfiguration config, RunStep step, string message) => new RunStepResult
        {
            Succeeded = false,
            BuildConfiguration = config,
            RunStep = step,
            RunInstance = null,
            Message = message
        };

        /// <summary>
        /// Create a new instance of <see cref="RunStepResult"/> that represent a failed execution.
        /// </summary>
        /// <param name="config">The <see cref="BuildConfiguration"/> used by the <see cref="Build.RunStep"/>.</param>
        /// <param name="step">The <see cref="Build.RunStep"/> that was executed.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>A new <see cref="RunStepResult"/> instance.</returns>
        public static RunStepResult Failure(BuildConfiguration config, RunStep step, Exception exception) => new RunStepResult
        {
            Succeeded = false,
            BuildConfiguration = config,
            RunStep = step,
            RunInstance = null,
            Exception = exception
        };

        public override string ToString() => $"Run {base.ToString()}";

        public void Dispose()
        {
            if (RunInstance != null)
            {
                RunInstance.Dispose();
            }
        }

        public RunStepResult() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("BuildSettings has been renamed to BuildConfiguration. (RemovedAfter 2020-05-01) (UnityUpgradable) -> BuildConfiguration")]
        public BuildConfiguration BuildSettings { get; internal set; }
    }
}
