using EngineCore.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECSTestProject
{
    public class TestComponent : IEntityComponent
    {
        public long Value;
        public int HorizontalDirection = 1;
        public int VerticalDirection = 1;
    }
}
