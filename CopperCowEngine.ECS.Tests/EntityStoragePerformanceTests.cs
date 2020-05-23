using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CopperCowEngine.ECS.Base;
using NUnit.Framework;

namespace CopperCowEngine.ECS.Tests
{
    [TestFixture]
    public class EntityStoragePerformanceTests
    {
        private const int N = 1000000;
        private const int K = UnmanagedEntitiesStorage.MaxEntitiesCount - 1;

        private EntitiesStorage _managed;

        private UnmanagedEntitiesStorage _unmanaged;

        private Stopwatch _stopwatch;

        [OneTimeSetUp]
        public void Init()
        {
            _managed = new EntitiesStorage();
            _unmanaged = new UnmanagedEntitiesStorage();
            _stopwatch = new Stopwatch();
        }

        [Test, Order(1)]
        public void CreateEntity()
        {
            _stopwatch.Start();
            for (var i = 0; i < K; i++)
            {
                var entity = _managed.CreateEntity();
            }
            TestContext.WriteLine($"Managed {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
            
            _stopwatch.Restart();
            for (var i = 0; i < K; i++)
            {
                var entity = _unmanaged.CreateEntity();
            }
            TestContext.WriteLine($"Unmanaged {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
        }

        [Test, Order(2)]
        public void GetEntity()
        {
            _stopwatch.Start();
            for (var i = 0; i < N; i++)
            {
                var entity = _managed.GetEntity(i % K);
            }
            TestContext.WriteLine($"Managed {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
            
            _stopwatch.Restart();
            for (var i = 0; i < N; i++)
            {
                var entity = _unmanaged.GetEntity(i % K);
            }
            TestContext.WriteLine($"Unmanaged {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
        }

        [Test, Order(3)]
        public void SetEntityArchetype()
        {
            _stopwatch.Start();
            for (var i = 0; i < N; i++)
            {
                _managed.SetEntityArchetype(Entity.Recycle(i % K, 0), 1, 1);
            }
            TestContext.WriteLine($"Managed {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
            
            _stopwatch.Restart();
            for (var i = 0; i < N; i++)
            {
                _unmanaged.SetEntityArchetype(Entity.Recycle(i % K, 0), 1, 1);
            }
            TestContext.WriteLine($"Unmanaged {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
        }

        [Test, Order(4)]
        public void GetEntityArchetypeIndex()
        {
            _stopwatch.Start();
            for (var i = 0; i < N; i++)
            {
                var t = _managed.GetEntityArchetypeIndex(Entity.Recycle(i % K, 0));
            }
            TestContext.WriteLine($"Managed {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
            
            _stopwatch.Restart();
            for (var i = 0; i < N; i++)
            {
                var t = _unmanaged.GetEntityArchetypeIndex(Entity.Recycle(i % K, 0));
            }
            TestContext.WriteLine($"Unmanaged {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
        }

        [Test, Order(5)]
        public void GetEntityArchetypeWithIndex()
        {
            _stopwatch.Start();
            for (var i = 0; i < N; i++)
            {
                var t = _managed.GetEntityArchetypeWithIndex(Entity.Recycle(i % K, 0));
            }
            TestContext.WriteLine($"Managed {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
            
            _stopwatch.Restart();
            for (var i = 0; i < N; i++)
            {
                var t = _unmanaged.GetEntityArchetypeWithIndex(Entity.Recycle(i % K, 0));
            }
            TestContext.WriteLine($"Unmanaged {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
        }
        
        [Test, Order(6)]
        public void DestroyEntity()
        {
            _stopwatch.Start();
            for (var i = 0; i < N; i++)
            {
                _managed.DestroyEntity(Entity.Recycle(i % K, 0));
            }
            TestContext.WriteLine($"Managed {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
            
            _stopwatch.Restart();
            for (var i = 0; i < N; i++)
            {
                _unmanaged.DestroyEntity(Entity.Recycle(i % K, 0));
            }
            TestContext.WriteLine($"Unmanaged {GetMicroseconds(_stopwatch.ElapsedMilliseconds, N)} μs");
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            _stopwatch.Stop();
            _unmanaged.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetMicroseconds(long ms, int max)
        {
            return (double) (ms * 1000000) / max;
        }
    }
}
