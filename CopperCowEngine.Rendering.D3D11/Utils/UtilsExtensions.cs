using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CopperCowEngine.Rendering.D3D11.Utils
{
    internal class D3DUtils
    {
        public static void WriteToDynamicBuffer(DeviceContext context, Buffer bufferRef, object value)
        {
            var dataRef = context.MapSubresource(bufferRef, 0, MapMode.WriteDiscard, MapFlags.None);
            Marshal.StructureToPtr(value, dataRef.DataPointer, false);
            context.UnmapSubresource(bufferRef, 0);
        }
    }
}
