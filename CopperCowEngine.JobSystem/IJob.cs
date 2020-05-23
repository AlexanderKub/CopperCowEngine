using System.Runtime.CompilerServices;

namespace CopperCowEngine.JobSystem
{
    /// <summary>
    /// IJob allows you to schedule a single job that runs in parallel to other jobs and the main thread.
    /// <br/><br/>When a job is scheduled, the job's Execute method will be invoked on a worker thread.
    /// The returned JobHandle can be used to ensure that the job has completed. Or it can be passed
    /// to other jobs as a dependency, thus ensuring the
    /// jobs are executed one after another on the worker threads.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Implement this method to perform work on a worker thread.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Execute();
    }
}
