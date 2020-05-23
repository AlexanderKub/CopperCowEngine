using System.Runtime.CompilerServices;

namespace CopperCowEngine.JobSystem
{
    public static class JobExtensions
    {
        /// <summary>
        /// Perform the job's Execute method immediately on the same thread.
        /// <br/>See Also: <seealso cref="IJob"/>
        /// </summary>
        /// <param name="jobData">The job and data to Run.</param>
        /// 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run<T>(this T jobData) where T : struct, IJob
        {
            jobData.Execute();
        }
        
        /// <summary>
        /// Schedule the job for execution on a worker thread.
        /// <br/>See Also: <seealso cref="IJob"/>
        /// </summary>
        /// <typeparam name="T">IJob structure</typeparam>
        /// <param name="jobData">The job and data to Run.</param>
        /// <param name="dependsOn">Dependencies are used to ensure that a job executes on worker threads after
        /// the dependency has completed execution. Making sure that two jobs reading or writing to same data
        /// do not run in parallel.</param>
        /// <returns><see cref="JobHandle"/> The handle identifying the scheduled job.
        /// <br/>Can be used as a dependency for a later job or ensure completion on the main thread.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle Schedule<T>(this T jobData, JobHandle dependsOn = default) where T : struct, IJob
        {
            return JobsTaskManager.Instance.ScheduleJob(jobData);
        }
    }
}
