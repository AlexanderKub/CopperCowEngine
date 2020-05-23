using System;
using System.Collections.Generic;
using System.Text;

namespace CopperCowEngine.ECS.Builtin.Singletons
{
    public struct ConsoleState : ISingletonComponentData
    {
        public bool IsShow;
        public int CommandHistoryIndex;
        public List<string> CommandHistory;
        public List<string> LogLines;
    }
}
