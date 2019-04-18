using EngineCore.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS.Components
{
    internal sealed class SingletonConfigVar : ISingletonEntityComponent
    {
        public int ScreenHeight;
        public int ScreenWidth;
        public float ScreenAspectRatio = 1;
        public bool IsInvertedDepthBuffer = true;//true
    }
}
