using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS.Exceptions
{
    public class ComponentAlreadyExistsException : Exception
    {
        public ComponentAlreadyExistsException() : base("Component already exists")
        {
        }
    }

    public class ComponentNotFoundException : Exception
    {
        public ComponentNotFoundException() : base("Component not found for entity")
        {
        }
    }

    public class TypeNotComponentException : Exception
    {
        public TypeNotComponentException() : base("Type does not extend IEntityComponent")
        {
        }
    }
}
