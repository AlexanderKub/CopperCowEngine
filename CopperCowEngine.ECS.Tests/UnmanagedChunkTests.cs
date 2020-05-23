using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CopperCowEngine.ECS.DataChunks;
using CopperCowEngine.Unsafe.Collections;
using NUnit.Framework;

namespace CopperCowEngine.ECS.Tests
{
    public struct TestComponent : IComponentData
    {
        public int A;
        public int B;
        public int C;
        public int D;

        public override string ToString()
        {
            return $"TestComponent {{ A = {A} B = {B} C = {C} D = {D} }}";
        }
    }
    
    public struct AnotherTestComponent : IComponentData
    {
        public int D;
        public int E;

        public override string ToString()
        {
            return $"AnotherTestComponent {{ D = {D} E = {E} }}";
        }
    }

    [TestFixture]
    public class UnmanagedChunkTests
    {
        [Test]
        public void CreateChunk()
        {
            var types = new[] { typeof(TestComponent), typeof(AnotherTestComponent) };
            var layout = CreateLayout(types, new[] { 1, 2 });
            var chunk = new UnmanagedChunk(layout);

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 10; i++)
            {
                var index = chunk.Add(i + 1);
                chunk.SetDataByIndex(index, new TestComponent
                {
                    A = 1 * (i + 1),
                    B = 1 * (i + 1),
                    C = 1 * (i + 1),
                    D = 1 * (i + 1),
                });
                chunk.SetDataByIndex(index, new AnotherTestComponent
                {
                    D = 1 * (i + 1),
                    E = 1 * (i + 1),
                });
            }
            stopwatch.Stop();
            TestContext.WriteLine($"{stopwatch.ElapsedMilliseconds} ms");
            chunk.RemoveByIndex(7);

            LogChunk(chunk, 10);
        }
        
        [Test]
        public void MoveDataToAnotherChunk()
        {
            var types = new[] { typeof(TestComponent), typeof(AnotherTestComponent) };
            var layout1 = CreateLayout(types, new[] { 1, 2 });

            types = new[] { typeof(AnotherTestComponent) };
            var layout2 = CreateLayout(types,new[] { 2 });

            var chunk1 = new UnmanagedChunk(layout1);
            var chunk2 = new UnmanagedChunk(layout2);
            
            for (var i = 0; i < 10; i++)
            {
                var index = chunk1.Add(i + 1);
                chunk1.SetDataByIndex(index, new TestComponent
                {
                    A = 1 * (i + 1),
                    B = 1 * (i + 1),
                    C = 1 * (i + 1),
                    D = 1 * (i + 1),
                });
                chunk1.SetDataByIndex(index, new AnotherTestComponent
                {
                    D = 1 * (i + 1),
                    E = 1 * (i + 1),
                });
            }

            chunk1.CopyDataToAnotherChunk(chunk2, 2, 0);

            LogChunk(chunk2, 10);
        }

        private static UnmanagedChunkLayoutElement[] CreateLayout(IReadOnlyList<Type> types, IReadOnlyList<int> ids)
        {
            var layout = new UnmanagedChunkLayoutElement[types.Count];

            for (var i = 0; i < layout.Length; i++)
            {
                var componentType = new ComponentType(types[i]);
                layout[i] = new UnmanagedChunkLayoutElement
                {
                    ItemSize = componentType.Size,
                    StartOffset = 0,
                    TypeHashCode = componentType.GetHashCode(),
                    TypeId = ids[i], //componentType.Id,
                };
            }

            return layout;
        }

        private static void LogChunk(UnmanagedChunk chunk, int n)
        {
            TestContext.WriteLine($"{chunk.Count}/{chunk.Capacity}");
            for (var i = 0; i < n; i++)
            {
                TestContext.WriteLine($"Entity {{ ID = {chunk.GetEntityIdByIndex(i)} }}");
                TestContext.WriteLine(chunk.GetDataByIndex<TestComponent>(i));
                TestContext.WriteLine(chunk.GetDataByIndex<AnotherTestComponent>(i));
            }
        }
    }
}
