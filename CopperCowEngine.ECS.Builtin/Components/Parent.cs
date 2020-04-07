using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct Parent : IComponentData
    {
        public Entity Value;
    }
}
