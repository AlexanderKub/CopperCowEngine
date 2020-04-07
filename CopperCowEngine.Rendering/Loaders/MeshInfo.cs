using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopperCowEngine.Rendering.Loaders
{
    public struct MeshInfo
    {
        public string Name { get; }
        public BoundsBox Bounds { get; }

        internal MeshInfo(string name, BoundsBox bounds)
        {
            Name = name;
            Bounds = bounds;
        }
    }
}
