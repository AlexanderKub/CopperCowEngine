using System.Diagnostics;
using CopperCowEngine.Unsafe.Collections;
using NUnit.Framework;

namespace CopperCowEngine.Unsafe.Tests
{
    internal struct TestStruct
    {
        public int A;
        public int B;
        public int C;
        public int D;
    }

    [TestFixture]
    public class PerformanceTest
    {
        private const int N = 1048 * 1024;
        
        private TestStruct[] _regularArray;

        private UnmanagedArray<TestStruct> _unmanagedArray;

        private Stopwatch _stopwatch;

        [SetUp]
        public void Setup()
        {
            _regularArray = new TestStruct[N];
            _unmanagedArray = new UnmanagedArray<TestStruct>(N);

            for (var i = 0; i < N; i++)
            {
                var data = new TestStruct
                {
                    A = 1,
                    B = i,
                    C = 1,
                    D = 1,
                };
                _regularArray[i] = data;
                _unmanagedArray[i] = data;
            }
            Assert.AreEqual(1, 1);
        }

        [Test]
        public void Read()
        {
            var sum = 0;
            _stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < N; i++)
            {
                sum += _regularArray[i].A;
            }
            _stopwatch.Stop();
            TestContext.WriteLine($"RegularArray: {_stopwatch.ElapsedMilliseconds} ms");
            Assert.AreEqual(N, sum);

            sum = 0;
            _stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < N; i++)
            {
                sum += _unmanagedArray[i].A;
            }
            _stopwatch.Stop();
            TestContext.WriteLine($"UnmanagedArray: {_stopwatch.ElapsedMilliseconds} ms");
            Assert.AreEqual(N, sum);
        }

        [Test]
        public void Write()
        {
            _stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < N; i++)
            {
                var r = _regularArray[i];
                r.A += 1;
                _regularArray[i] = r;
            }
            _stopwatch.Stop();
            TestContext.WriteLine($"RegularArray: {_stopwatch.ElapsedMilliseconds} ms");
            Assert.AreEqual(2, _regularArray[N / 2].A);

            _stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < N; i++)
            {
                var r = _unmanagedArray[i];
                r.A += 1;
                _unmanagedArray[i] = r;
            }
            _stopwatch.Stop();
            TestContext.WriteLine($"UnmanagedArray: {_stopwatch.ElapsedMilliseconds} ms");
            Assert.AreEqual(2, _unmanagedArray[N / 2].A);
        }

        [Test]
        public void Length()
        {
            int length;

            _stopwatch = Stopwatch.StartNew();
            length = _regularArray.Length;
            _stopwatch.Stop();
            TestContext.WriteLine($"RegularArray: {_stopwatch.ElapsedMilliseconds} ms");
            Assert.AreEqual(N, length);

            _stopwatch = Stopwatch.StartNew();
            length = _unmanagedArray.Length;
            _stopwatch.Stop();
            TestContext.WriteLine($"UnmanagedArray: {_stopwatch.ElapsedMilliseconds} ms");
            Assert.AreEqual(N, length);
        }

        [TearDown]
        public void DisposeArrays()
        {
            _unmanagedArray.Dispose();
        }
    }
}
