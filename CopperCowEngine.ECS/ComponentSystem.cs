﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CopperCowEngine.ECS
{
    public abstract class ComponentSystem : IDisposable
    {
        public bool Enable;

        protected EntitiesIterator Iterator { get; private set; }

        protected EcsContext Context { get; private set; }

        /// <summary>
        /// Immediate <see cref="CommandContext"/> would be executed after the current system update.
        /// </summary>
        protected CommandContext ImmediateContext { get; private set; }

        /// <summary>
        /// Deferred <see cref="CommandContext"/> would be executed at the beginning of the next frame.
        /// </summary>
        protected internal CommandContext DeferredContext { get; private set; }

        public void Dispose()
        {
            OnDestroy();
        }

        internal abstract void InternalInit(EcsContext context);

        internal void InternalUpdate()
        {
            if (!Enable)
            {
                return;
            }
            Update();
            ImmediateContext.Execute();
        }

        internal virtual void Init(EcsContext context, Type[] required, Type[] optional, Type[] excepted)
        {
            Context = context;
            ImmediateContext = new CommandContext(context);
            DeferredContext = new CommandContext(context);
            Enable = true;

            if (required == null && optional == null && excepted == null)
            {
                Iterator = null;
                OnCreate();
                return;
            }

            Iterator = new EntitiesIterator(Context, required, optional, excepted);
            OnCreate();
        }
        
        protected virtual void OnCreate() { }

        protected virtual void OnDestroy() { }
        
        /// <summary>
        /// This method will be called in the context update loop, according to the system update order.
        /// </summary>
        protected abstract void Update();
        
        protected class EntitiesIterator : IEnumerable
        {
            private readonly EcsContext _context;

            private readonly int[] _required;
            private readonly int[] _optional;
            private readonly int[] _excepted;

            public EntitiesIterator(EcsContext context, IEnumerable<Type> required, IEnumerable<Type> optional, IEnumerable<Type> excepted)
            {
                _context = context;

                // TODO: Refactoring
                _required = required?.Select(t => context.DataChunkStorage.TypesStorage.TryRegisterType(t)).ToArray();
                _optional = optional?.Select(t => context.DataChunkStorage.TypesStorage.TryRegisterType(t)).ToArray();
                _excepted = excepted?.Select(t => context.DataChunkStorage.TypesStorage.TryRegisterType(t)).ToArray();
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

            private readonly int[] _required;
            private readonly int[] _optional;
            private readonly int[] _excepted;

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

            public EntityIEnumerator(EcsContext context, int[] required, int[] optional, int[] excepted)
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
                        // TODO: Dispose empty chunks
                        chunkChainEnd = _context.DataChunkStorage.ChunksStorage[_archetypePosition].Chunks[_chunkPosition].Count == 0;
                        if (!chunkChainEnd)
                        {
                            return true;
                        }
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
