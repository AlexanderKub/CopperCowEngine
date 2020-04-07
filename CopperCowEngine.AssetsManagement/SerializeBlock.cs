using System.Runtime.InteropServices;

namespace CopperCowEngine.AssetsManagement
{
    internal class SerializeBlock
    {
        public static byte[] GetBytes<T>(T str) {
            var size = Marshal.SizeOf(str);
            var bytesArray = new byte[size];
            var handle = default(GCHandle);

            try {
                handle = GCHandle.Alloc(bytesArray, GCHandleType.Pinned);
                Marshal.StructureToPtr<T>(str, handle.AddrOfPinnedObject(), false);
            } finally {
                if (handle.IsAllocated) {
                    handle.Free();
                }
            }
            return bytesArray;
        }

        public static T FromBytes<T>(byte[] arr) where T : struct {
            T str;
            var handle = default(GCHandle);

            try {
                handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
                str = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            } finally {
                if (handle.IsAllocated) {
                    handle.Free();
                }
            }
            return str;
        }
    }
}
