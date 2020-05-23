using System;
using System.Runtime.InteropServices;
#if DEBUG
using System.Collections.Generic;
#endif

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

        public static IntPtr MemAlloc(int size, int stride, string debugLabel = "unnamed allocation")
        {
            var pointer = Marshal.AllocHGlobal(size * stride);
            #if DEBUG
            Allocations.Add(pointer);
            #endif

            var span = new Span<byte>(pointer.ToPointer(), size * stride);
            span.Fill(0);

            return pointer;
        }

        public static void MemCopy(IntPtr sourcePointer, IntPtr destinationPointer, int length)
        {
           Buffer.MemoryCopy(sourcePointer.ToPointer(), destinationPointer.ToPointer(), length, length);
        }

        public static void MemFree(IntPtr pointer)
        {
            #if DEBUG
            Allocations.Remove(pointer);
            #endif
            Marshal.FreeHGlobal(pointer);
        }
        
        public static T[] GetArray<T>(IntPtr pointer, int length) where T : struct 
        {
            var span = new Span<T>(pointer.ToPointer(), length);

            return span.ToArray();
        }

        public static ref T ElementDirect<T>(IntPtr pointer, int index) where T : unmanaged 
        {
            return ref *(T*)(pointer + index);
        }

        public static T ReadElement<T>(IntPtr pointer, int index) where T : unmanaged 
        {
            return *((T*)pointer + index);
        }

        public static void WriteElement<T>(IntPtr pointer, int index, T value) where T : unmanaged
        {
            *((T*)pointer + index) = value;
        }

        public static void WriteElementDirectP<T>(IntPtr pointer, int index, in T value) where T : unmanaged
        {
            *(T*)(pointer + index) = value;
        }

        /*
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

            return (IntPtr)pRef; //(&pRef)
        }
        */
    }
}
