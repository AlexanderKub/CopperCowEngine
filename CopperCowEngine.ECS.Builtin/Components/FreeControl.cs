using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct FreeControl : IComponentData
    {
        public float Yaw;
        public float Pitch;
        public float Roll;
        public float Speed;
    }
}
