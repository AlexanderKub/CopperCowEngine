using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Core;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.Rendering;
using CopperCowEngine.Rendering.D3D11;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Loaders;
using CopperCowEngine.Unsafe;
using CopperCowEngine.Unsafe.Collections;

namespace PureProject
{
    internal class Program
    {
        private static int[] _source = new[] {88, 1, 2, 3, 4, 5};

        private static void Main(string[] args)
        {
            var game = new Game();

            /*var elementSize = Marshal.SizeOf<LocalToWorld>();
            elementSize += Marshal.SizeOf<LocalToParent>();
            elementSize += Marshal.SizeOf<Parent>();
            elementSize += Marshal.SizeOf<Translation>();
            elementSize += Marshal.SizeOf<Rotation>();
            elementSize += Marshal.SizeOf<Scale>();
            elementSize += Marshal.SizeOf<CameraViewProjection>();
            elementSize += Marshal.SizeOf<CameraSetup>();*/
            //UnmanagedArray<LocalToWorld>.Allocate(16 * elementSize, out var array);
            // var array = new UnmanagedArray<int>(_source);
            //var test = array.ToArray();
            //array.Dispose();
            /*var mapping = new UnmanagedChunkMapping[]
            {
                new UnmanagedChunkMapping()
                {
                    TypeId = 1,
                    ElementSize = elementSize,
                    StartOffset = 0,
                },
            };
            var chunk = new UnmanagedChunk(mapping);
            UnsafeUtility.Cleanup();*/
        }

        private static void PreRenderTools()
        {
            var path = "C:\\Repos\\CopperCowEngine\\RawContent\\Mt-Washington-Cave-Room_Ref.hdr";
            //path = "C:\\Repos\\CopperCowEngine\\RawContent\\Tokyo_BigSight_3k.hdr";
            AssetsManager.GetManager().CubeMapPrerender(path, "House");
        }
    }
}
