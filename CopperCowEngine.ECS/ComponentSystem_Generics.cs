using System;

namespace CopperCowEngine.ECS
{
    public abstract class ComponentSystem<R> : ComponentSystem where R : Required
    {
        internal sealed override void InternalInit(EcsContext context)
        {
            Init(context, typeof(R).GetGenericArguments(), null, null);
        }
    }

    public abstract class ComponentSystem<R, O> : ComponentSystem where R : Required where O : Optional
    {
        internal sealed override void InternalInit(EcsContext context)
        {
            Init(context, typeof(R).GetGenericArguments(), typeof(O).GetGenericArguments(), null);
        }
    }

    public abstract class ComponentSystem<R, O, E> : ComponentSystem where R : Required where O : Optional where E : Excepted
    {
        internal sealed override void InternalInit(EcsContext context)
        {
            Init(context, typeof(R).GetGenericArguments(), typeof(O).GetGenericArguments(), typeof(E).GetGenericArguments());
        }
    }

    public abstract class ComponentlessSystem : ComponentSystem<Required>
    {
        internal override void Init(EcsContext context, Type[] required, Type[] optional, Type[] excepted)
        {
            base.Init(context, null, null, null);
        }

        [Obsolete("Iterator is not supported in ComponentlessSystem.", true)]
        protected new EntitiesIterator Iterator;
    }
}
