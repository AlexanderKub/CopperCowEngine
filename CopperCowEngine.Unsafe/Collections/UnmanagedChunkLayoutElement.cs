using System;
using System.Collections.Generic;
using System.Text;

namespace CopperCowEngine.Unsafe.Collections
{
    public struct UnmanagedChunkLayoutElement
    {
        public int TypeId;
        public int TypeHashCode;
        public int StartOffset;
        public int ItemSize;
    }
}
