using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CopperCowEngine.JobSystem
{
    internal class JobsTaskManager
    {
        public static JobsTaskManager Instance { get; } = new JobsTaskManager();

        private readonly List<Task> _taskList;

        static JobsTaskManager() { }

        private JobsTaskManager()
        {
            _taskList = new List<Task>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle ScheduleJob<T>(T jobData) where T : struct, IJob
        {
            var task = new Task(jobData.Execute);

            _taskList.Add(task);
            
            return new JobHandle
            {
                Id = task.Id,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle ScheduleParallelForJob<T>(T jobData, int arrayLength, int batchCount) where T : struct, IJobParallelFor
        {
            var batchSize = arrayLength / batchCount;


            var task = new Task(() =>
            {
                for (var i = 0; i < batchCount; i++)
                {
                    var i1 = i;
                    Task.Factory.StartNew(() =>
                    {
                        for (var j = 0; j < batchSize && batchSize * i1 + j < arrayLength; j++)
                        {
                            jobData.Execute(batchSize * i1 + j);
                        }
                    }, TaskCreationOptions.AttachedToParent);
                }
            });

            _taskList.Add(task);
            
            return new JobHandle
            {
                Id = task.Id,
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ScheduleBatchedJobsAndComplete(ref JobHandle handle)
        {
            var taskId = handle.Id;

            if (taskId == -1)
            {
                return;
            }

            var task = _taskList.Find(t => t.Id == taskId);

            if (task == null || task.IsCompleted)
            {
                handle.Id = -1;
                return;
            }

            if (task.Status == TaskStatus.Created)
            {
                task.Start();
                task.Wait();
            }

            _taskList.Remove(task);
            handle.Id = -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool ScheduleBatchedJobsAndCompleted(ref JobHandle handle)
        {
            var taskId = handle.Id;

            if (taskId == -1)
            {
                return true;
            }

            var task = _taskList.Find(t => t.Id == taskId);

            if (task == null || task.IsCompleted)
            {
                handle.Id = -1;
                return true;
            }

            if (task.Status == TaskStatus.Created)
            {
                task.Start();
            }

            return task.IsCompleted;
        }
    }
}
