using System.Diagnostics;
using System.Runtime.CompilerServices;
using CopperCowEngine.Unsafe.Collections;
using NUnit.Framework;

namespace CopperCowEngine.JobSystem.Tests
{
    internal struct TestParallelJob : IJobParallelFor
    {
        public UnmanagedArray<float> A;
        public UnmanagedArray<float> B;
        public UnmanagedArray<float> Result;

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(int i)
        {
            Result[i] = A[i] + B[i];
        }
    }

    [TestFixture]
    public class JobsTestParallelFor
    {
        private TestParallelJob _job;
        private Stopwatch _stopwatch;
        private const int N = 4096 * 1024;

        [SetUp]
        public void CreateJob()
        {
            var a = new UnmanagedArray<float>(N);
            var b = new UnmanagedArray<float>(N);
            var result = new UnmanagedArray<float>(N);

            for (var i = 0; i < N; i++)
            {
                a[i] = i;
                b[i] = i;
            }

            _job = new TestParallelJob
            {
                A = a,
                B = b,
                Result = result,
            };
            Assert.AreEqual(1.0f, 1.0f);
        }

        [Test]
        public void RunJob()
        {
            _stopwatch = Stopwatch.StartNew();
            _job.Run(N);

            Assert.AreEqual(_job.A[1] + _job.B[1], _job.Result[1]);
            
            _stopwatch.Stop();
            TestContext.WriteLine($"{nameof(RunJob)}: {_stopwatch.ElapsedMilliseconds} ms");
        }

        [Test]
        public void ScheduleJob()
        {
            _stopwatch = Stopwatch.StartNew();
            var jobHandle = _job.Schedule(N, 64);
            jobHandle.Complete();
            
            Assert.AreEqual(_job.A[1] + _job.B[1], _job.Result[1]);

            _stopwatch.Stop();
            TestContext.WriteLine($"{nameof(ScheduleJob)}: {_stopwatch.ElapsedMilliseconds} ms");
        }

        [TearDown]
        public void DisposeJob()
        {
            _job.A.Dispose();
            _job.B.Dispose();
            _job.Result.Dispose();
        }
    }
}
