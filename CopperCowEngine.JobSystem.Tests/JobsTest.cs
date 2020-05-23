using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using CopperCowEngine.Unsafe.Collections;
using NUnit.Framework;

namespace CopperCowEngine.JobSystem.Tests
{
    internal struct VelocityJob : IJob
    {
        public UnmanagedArray<Vector3> Velocity;

        public UnmanagedArray<Vector3> Position;

        public float DeltaTime;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Execute()
        {
            for (var i = 0; i < Position.Length; i++)
            {
                Position[i] = Position[i] + Velocity[i] * DeltaTime;
            }
        }
    }

    [TestFixture]
    public class JobsTest
    {
        private VelocityJob _job;
        private Stopwatch _stopwatch;

        [SetUp]
        public void CreateJob()
        {
            const int n = 1024 * 1024;
            var position = new UnmanagedArray<Vector3>(n);

            var velocity = new UnmanagedArray<Vector3>(n);

            for (var i = 0; i < velocity.Length; i++)
            {
                velocity[i] = new Vector3(0, 10, 0);
            }

            _job = new VelocityJob()
            {
                DeltaTime = 0.01f,
                Position = position,
                Velocity = velocity
            };
            Assert.AreEqual(1.0f, 1.0f);
        }

        [Test]
        public void RunJob()
        {
            _stopwatch = Stopwatch.StartNew();
            _job.Run();

            Assert.AreEqual(10f * 0.01f, _job.Position[0].Y);

            _stopwatch.Stop();
            TestContext.WriteLine($"{nameof(RunJob)}: {_stopwatch.ElapsedMilliseconds} ms");
        }

        [Test]
        public void ScheduleJob()
        {
            _stopwatch = Stopwatch.StartNew();
            var jobHandle = _job.Schedule();

            jobHandle.Complete();

            Assert.AreEqual(10f * 0.01f, _job.Position[0].Y);

            _stopwatch.Stop();
            TestContext.WriteLine($"{nameof(ScheduleJob)}: {_stopwatch.ElapsedMilliseconds} ms");
        }

        [Test]
        public void ScheduleJobAndWaitComplete()
        {
            _stopwatch = Stopwatch.StartNew();
            var jobHandle = _job.Schedule();

            while (!jobHandle.IsCompleted)
            {
                Thread.Sleep(1);
            }

            Assert.AreEqual(10f * 0.01f, _job.Position[0].Y);

            _stopwatch.Stop();
            TestContext.WriteLine($"{nameof(ScheduleJobAndWaitComplete)}: {_stopwatch.ElapsedMilliseconds} ms");
        }

        [Test]
        public void ScheduleTwoJobAndWaitComplete()
        {
            _stopwatch = Stopwatch.StartNew();
            var jobHandle = _job.Schedule();
            var jobHandle2 = _job.Schedule();

            var t = jobHandle.IsCompleted;
            jobHandle2.Complete();
            jobHandle.Complete();

            Assert.AreEqual(20f * 0.01f, _job.Position[0].Y);

            _stopwatch.Stop();
            TestContext.WriteLine($"{nameof(ScheduleJobAndWaitComplete)}: {_stopwatch.ElapsedMilliseconds} ms");
        }

        [TearDown]
        public void DisposeJob()
        {
            _job.Position.Dispose();
            _job.Velocity.Dispose();
        }
    }
}
