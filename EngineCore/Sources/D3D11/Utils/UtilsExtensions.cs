using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace EngineCore.D3D11
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
