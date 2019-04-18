using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS
{
    /// <summary>
    /// A base interface for components.
    /// </summary>
    public interface IEntityComponent { }

    /// <summary>
    /// An interface for singleton components.
    /// </summary>
    public interface ISingletonEntityComponent { }

    /// <summary>
    /// An interface for components with Entity id injected.
    /// </summary>
    public interface IEntityComponentWithEntityId {
        int EntityId { get; set; }
    }
}
