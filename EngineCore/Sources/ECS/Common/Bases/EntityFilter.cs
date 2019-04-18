using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS
{
    /// <summary>
    /// Abstract class for entity filtering in systems.
    /// </summary>
    public abstract class EntityFilter {
        protected EntityFilter() { }
    }

    #region Generated code
    //*_____START_OF_GENERATED_CODE_____*//
    public class Requires : EntityFilter { }
    public class Requires<T> : Requires where T : IEntityComponent { }
    public class Requires<T, Y> : Requires where T : IEntityComponent where Y : IEntityComponent { }
    public class Requires<T, Y, U> : Requires where T : IEntityComponent where Y : IEntityComponent where U : IEntityComponent { }
    public class Requires<T, Y, U, I> : Requires where T : IEntityComponent where Y : IEntityComponent where U : IEntityComponent where I : IEntityComponent { }
    public class Requires<T, Y, U, I, O> : Requires where T : IEntityComponent where Y : IEntityComponent where U : IEntityComponent where I : IEntityComponent where O : IEntityComponent { }

    public class Excludes : EntityFilter { }
    public class Excludes<T> : Excludes where T : IEntityComponent { }
    public class Excludes<T, Y> : Excludes where T : IEntityComponent where Y : IEntityComponent { }
    public class Excludes<T, Y, U> : Excludes where T : IEntityComponent where Y : IEntityComponent where U : IEntityComponent { }
    public class Excludes<T, Y, U, I> : Excludes where T : IEntityComponent where Y : IEntityComponent where U : IEntityComponent where I : IEntityComponent { }
    public class Excludes<T, Y, U, I, O> : Excludes where T : IEntityComponent where Y : IEntityComponent where U : IEntityComponent where I : IEntityComponent where O : IEntityComponent { }

    public class RequiresOne : EntityFilter { }
    public class RequiresOne<T> : RequiresOne where T : IEntityComponent { }
    public class RequiresOne<T, Y> : RequiresOne where T : IEntityComponent where Y : IEntityComponent { }
    public class RequiresOne<T, Y, U> : RequiresOne where T : IEntityComponent where Y : IEntityComponent where U : IEntityComponent { }
    public class RequiresOne<T, Y, U, I> : RequiresOne where T : IEntityComponent where Y : IEntityComponent where U : IEntityComponent where I : IEntityComponent { }
    public class RequiresOne<T, Y, U, I, O> : RequiresOne where T : IEntityComponent where Y : IEntityComponent where U : IEntityComponent where I : IEntityComponent where O : IEntityComponent { }
    //*_____END_OF_GENERATED_CODE_____*//
    #endregion
}
