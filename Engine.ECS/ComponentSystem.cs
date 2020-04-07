using System;
using System.Collections;
using System.Collections.Generic;

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

    public abstract class ComponentSystem
    {
        protected EntitiesIterator Iterator { get; private set; }

        protected EcsContext Context { get; private set; }

        protected internal virtual void InternalInit(EcsContext context, Type[] required, Type[] optional, Type[] excepted)
        {
            Context = context;

            Iterator = new EntitiesIterator(Context, required, optional, excepted);
        }

        internal abstract void Init(EcsContext context);

        internal abstract void InternalUpdate();

        protected class EntitiesIterator : IEnumerable
        {
            private readonly EcsContext _context;

            private readonly Type[] _required;
            private readonly Type[] _optional;
            private readonly Type[] _excepted;

            public EntitiesIterator(EcsContext context, Type[] required, Type[] optional, Type[] excepted)
            {
                _context = context;
                _required = required;
                _optional = optional;
                _excepted = excepted;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public EntityIEnumerator GetEnumerator()
            {
                return new EntityIEnumerator(_context, _required, _optional, _excepted);
            }
        }

        protected class EntityIEnumerator : IEnumerator<ComponentsSlice>
        {
            private readonly EcsContext _context;

            private readonly Type[] _required;
            private readonly Type[] _optional;
            private readonly Type[] _excepted;

            private int _archetypePosition;

            private int _chunkPosition;

            private int _entityPosition = -1;

            private bool _archetypeChanged = true;

            public ComponentsSlice Current
            {
                get
                {
                    try
                    {
                        return new ComponentsSlice(_context, _archetypePosition, _chunkPosition, _entityPosition);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            object IEnumerator.Current => Current;

            public EntityIEnumerator(EcsContext context, Type[] required, Type[] optional, Type[] excepted)
            {
                _context = context;
                _required = required;
                _optional = optional;
                _excepted = excepted;
            }

            public bool MoveNext()
            {
                while (true)
                {
                    if (_context.DataChunkStorage.ChunksStorage.Count == 0)
                    {
                        return false;
                    }

                    if (_archetypeChanged)
                    {
                        var n = _context.DataChunkStorage.ArchetypesStorage.Count;
                        while (_archetypePosition < n)
                        {
                            ref var archetype = ref _context.DataChunkStorage.ArchetypesStorage.GetAt(_archetypePosition);

                            if (archetype.Filter(_required, _optional, _excepted))
                            {
                                break;
                            }

                            if (_archetypePosition == n - 1)
                            {
                                return false;
                            }

                            _archetypePosition++;
                        }
                    }

                    _archetypeChanged = false;

                    _entityPosition++;

                    var chunkEnd = _entityPosition == _context.DataChunkStorage.ChunksStorage[_archetypePosition].Chunks[_chunkPosition].Count;

                    if (chunkEnd)
                    {
                        _entityPosition = 0;
                        _chunkPosition++;
                    }
                    else
                    {
                        return true;
                    }

                    var chunkChainEnd = _chunkPosition == _context.DataChunkStorage.ChunksStorage[_archetypePosition].Count;

                    if (!chunkChainEnd)
                    {
                        return true;
                    }

                    _entityPosition = -1;
                    _chunkPosition = 0;
                    _archetypePosition++;

                    if (_archetypePosition == _context.DataChunkStorage.ArchetypesStorage.Count)
                    {
                        return false;
                    }

                    _archetypeChanged = true;
                }
            }

            public void Reset()
            {
                _archetypePosition = 0;
                _chunkPosition = 0;
                _entityPosition = -1;
            }

            public void Dispose()
            {
                // TODO: deferred operations
                // _context.PerformDeferredOperations();
            }
        }
    }
}
