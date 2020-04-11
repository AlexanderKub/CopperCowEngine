using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CopperCowEngine.ECS
{
    public abstract class ComponentSystem<R> : ComponentSystem where R : Required
    {
        internal sealed override void Init(EcsContext context)
        {
            base.InternalInit(context, typeof(R).GetGenericArguments(), null, null);
        }

        internal sealed override void InternalUpdate()
        {
            Update();
        }
        protected abstract void Update();
    }

    public abstract class ComponentSystem<R, O> : ComponentSystem where R : Required where O : Optional
    {
        internal sealed override void Init(EcsContext context)
        {
            base.InternalInit(context, typeof(R).GetGenericArguments(), typeof(O).GetGenericArguments(), null);
        }

        internal sealed override void InternalUpdate()
        {
            Update();
        }
        protected abstract void Update();
    }

    public abstract class ComponentSystem<R, O, E> : ComponentSystem where R : Required where O : Optional where E : Excepted
    {
        internal sealed override void Init(EcsContext context)
        {
            base.InternalInit(context, typeof(R).GetGenericArguments(), typeof(O).GetGenericArguments(), typeof(E).GetGenericArguments());
        }

        internal sealed override void InternalUpdate()
        {
            Update();
        }
        protected abstract void Update();
    }

    public abstract class ComponentlessSystem : ComponentSystem<Required>
    {
        protected internal override void InternalInit(EcsContext context, Type[] required, Type[] optional, Type[] excepted)
        {
            base.InternalInit(context, null, null, null);
        }
    }
}
