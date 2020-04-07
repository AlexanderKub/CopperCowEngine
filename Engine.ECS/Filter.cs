
namespace CopperCowEngine.ECS
{
    public abstract class Required { }
    public abstract class Required<T> : Required 
        where T : struct, IComponentData { }
    public abstract class Required<T1, T2> : Required 
        where T1 : struct, IComponentData 
        where T2 : struct, IComponentData { }
    public abstract class Required<T1, T2, T3> : Required 
        where T1 : struct, IComponentData 
        where T2 : struct, IComponentData 
        where T3 : struct, IComponentData { }
    public abstract class Required<T1, T2, T3, T4> : Required
        where T1 : struct, IComponentData
        where T2 : struct, IComponentData
        where T3 : struct, IComponentData
        where T4 : struct, IComponentData
    { }
    public abstract class Required<T1, T2, T3, T4, T5> : Required
        where T1 : struct, IComponentData
        where T2 : struct, IComponentData
        where T3 : struct, IComponentData
        where T4 : struct, IComponentData
        where T5 : struct, IComponentData
    { }

    public abstract class Optional { }
    public abstract class Optional<T> : Optional
        where T : struct, IComponentData
    { }
    public abstract class Optional<T1, T2> : Optional
        where T1 : struct, IComponentData
        where T2 : struct, IComponentData
    { }
    public abstract class Optional<T1, T2, T3> : Optional
        where T1 : struct, IComponentData
        where T2 : struct, IComponentData
        where T3 : struct, IComponentData
    { }
    public abstract class Optional<T1, T2, T3, T4> : Optional
        where T1 : struct, IComponentData
        where T2 : struct, IComponentData
        where T3 : struct, IComponentData
        where T4 : struct, IComponentData
    { }
    public abstract class Optional<T1, T2, T3, T4, T5> : Optional
        where T1 : struct, IComponentData
        where T2 : struct, IComponentData
        where T3 : struct, IComponentData
        where T4 : struct, IComponentData
        where T5 : struct, IComponentData
    { }

    public abstract class Excepted { }
    public abstract class Excepted<T> : Excepted
        where T : struct, IComponentData
    { }
    public abstract class Excepted<T1, T2> : Excepted
        where T1 : struct, IComponentData
        where T2 : struct, IComponentData
    { }
    public abstract class Excepted<T1, T2, T3> : Excepted
        where T1 : struct, IComponentData
        where T2 : struct, IComponentData
        where T3 : struct, IComponentData
    { }
}
