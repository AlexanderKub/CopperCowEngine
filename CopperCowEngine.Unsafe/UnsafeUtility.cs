using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/*namespace System
{
    public static class Test
    {
        public static T Method<T>()
        {
            PtrUtils.ElemOffset<T>(new T[1]);
        }
    }
}*/
namespace CopperCowEngine.Unsafe
{
    public static unsafe class UnsafeUtility
    {
        #if DEBUG
        private static readonly HashSet<IntPtr> Allocations = new HashSet<IntPtr>();
        #endif
        
        
        public static void Cleanup()
        {
            #if DEBUG
            foreach (var ptr in Allocations)
            {
                Marshal.FreeHGlobal(ptr);
            }
            Allocations.Clear();
            #endif
        }

        public static void* MemAlloc(int size, int stride, string debugLabel = "unnamed allocation")
        {
            var pointer = Marshal.AllocHGlobal(size * stride);
            #if DEBUG
            Allocations.Add(pointer);
            #endif
            return (void*) pointer;
        }

        public static void MemClear(void* buffer)
        {
        }

        public static void MemCopy(void* source, void* destination, int length)
        {
           Buffer.MemoryCopy(source, destination, length, length);
        }

        public static void MemFree(void* buffer)
        {
            var pointer = (IntPtr) buffer;
            #if DEBUG
            Allocations.Remove(pointer);
            #endif
            Marshal.FreeHGlobal(pointer);
        }

        public static T ReadElement<T>(void* buffer, int index) where T : struct 
        {
            var elementSize = Marshal.SizeOf<T>();

            var span = new Span<T>(((byte*) buffer) + index * elementSize, 1);

            return span.GetPinnableReference();
        }

        public static void WriteElement<T>(void* buffer, int index, T value) where T : struct
        {
            var elementSize = Marshal.SizeOf<T>();

            var span = new Span<T>(((byte*) buffer) + index * elementSize, 1);

            span.GetPinnableReference() = value;
        }
        /// <summary>
        /// Provides the current address of the given element
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static IntPtr AddressOf<T>(T t)
            //remember ReferenceTypes are references to the CLRHeader
            //where TOriginal : struct
        {
            var reference = __makeref(t);

            return *(IntPtr*)(&reference);
        }
        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static IntPtr AddressOfRef<T>(ref T t)
            //remember ReferenceTypes are references to the CLRHeader
            //where TOriginal : struct
        {
            var reference = __makeref(t);

            var pRef = &reference;

            return (System.IntPtr)pRef; //(&pRef)
        }
    }
}
