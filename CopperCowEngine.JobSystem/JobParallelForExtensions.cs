using System.Runtime.CompilerServices;

namespace CopperCowEngine.JobSystem
{
    public static class JobParallelForExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Run<T>(this T jobData, int arrayLength) where T : struct, IJobParallelFor
        {
            for (var i = 0; i < arrayLength; i++)
            {
                jobData.Execute(i);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JobHandle Schedule<T>(this T jobData, int arrayLength, int batchCount, JobHandle dependsOn = default) where T : struct, IJobParallelFor
        {
            return JobsTaskManager.Instance.ScheduleParallelForJob(jobData, arrayLength, batchCount);
        }
    }
}
