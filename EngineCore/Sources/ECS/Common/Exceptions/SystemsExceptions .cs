using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS.Exceptions
{
    public class SystemAlreadyExistsException : Exception
    {
        public SystemAlreadyExistsException() : base("System already exists")
        {
        }
    }

    public class SystemNotFoundException : Exception
    {
        public SystemNotFoundException() : base("System not found")
        {
        }
    }

    public class TypeNotSystemException : Exception
    {
        public TypeNotSystemException() : base("Type does not extend BaseSystem")
        {
        }
    }
}
