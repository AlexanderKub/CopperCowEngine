using System.Runtime.CompilerServices;

namespace CopperCowEngine.JobSystem
{
    public struct JobHandle
    {
        internal int Id;

        /// <summary>
        /// Returns false if the task is currently running. Returns true if the task has completed.
        /// </summary>
        public bool IsCompleted => JobsTaskManager.Instance.ScheduleBatchedJobsAndCompleted(ref this);

        /// <summary>
        /// Ensures that the job has completed.
        /// The JobSystem automatically prioritizes the job and any of its dependencies to run
        /// first in the queue, then attempts to execute the job itself on the thread which
        /// calls the Complete function.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Complete()
        {
            JobsTaskManager.Instance.ScheduleBatchedJobsAndComplete(ref this);
        }
    }
}
