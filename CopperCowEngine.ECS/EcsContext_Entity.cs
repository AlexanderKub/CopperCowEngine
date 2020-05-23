using System;
using System.Diagnostics.CodeAnalysis;

namespace CopperCowEngine.ECS
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public partial class EcsContext
    {
        // TODO: Any better way to create entities?
        // CreateEntity(Archetype, T1,...,Tn data);
        
        private ArchetypeIndexTuple AddToArchetypeAndGetIndex(Entity entity, params Type[] types)
        {
            var newArchetype = DataChunkStorage.GetArchetype(types);

            var archetypeIndex = DataChunkStorage.TryRegisterArchetype(in newArchetype);

            var indexInArchetype = DataChunkStorage.AddEntity(entity, archetypeIndex);

            return new ArchetypeIndexTuple(archetypeIndex, indexInArchetype);
        }

        public Entity CreateEntityWithoutData(params Type[] types)
        {
            var entity = EntitiesStorage.CreateEntity();

            var index = AddToArchetypeAndGetIndex(entity, types);

            EntitiesStorage.SetEntityArchetype(entity, index.ArchetypeIndex, index.IndexInArchetype);

            return entity;
        }

        public Entity CreateEntity<T>(T data) where T : unmanaged, IComponentData
        {
            var entity = EntitiesStorage.CreateEntity();

            var index = AddToArchetypeAndGetIndex(entity, typeof(T));

            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data);

            EntitiesStorage.SetEntityArchetype(entity, index.ArchetypeIndex, index.IndexInArchetype);

            return entity;
        }

        public Entity CreateEntity<T1, T2>(T1 data1, T2 data2)
            where T1 : unmanaged, IComponentData
            where T2 : unmanaged, IComponentData
        {
            var entity = EntitiesStorage.CreateEntity();

            var index = AddToArchetypeAndGetIndex(entity, typeof(T1), typeof(T2));

            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data1);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data2);

            EntitiesStorage.SetEntityArchetype(entity, index.ArchetypeIndex, index.IndexInArchetype);

            return entity;
        }

        public Entity CreateEntity<T1, T2, T3>(T1 data1, T2 data2, T3 data3)
            where T1 : unmanaged, IComponentData
            where T2 : unmanaged, IComponentData
            where T3 : unmanaged, IComponentData
        {
            var entity = EntitiesStorage.CreateEntity();

            var index = AddToArchetypeAndGetIndex(entity, typeof(T1), typeof(T2), typeof(T3));

            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data1);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data2);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data3);

            EntitiesStorage.SetEntityArchetype(entity, index.ArchetypeIndex, index.IndexInArchetype);

            return entity;
        }

        public Entity CreateEntity<T1, T2, T3, T4>(T1 data1, T2 data2, T3 data3, T4 data4)
            where T1 : unmanaged, IComponentData
            where T2 : unmanaged, IComponentData
            where T3 : unmanaged, IComponentData
            where T4 : unmanaged, IComponentData
        {
            var entity = EntitiesStorage.CreateEntity();

            var index = AddToArchetypeAndGetIndex(entity, typeof(T1), typeof(T2), typeof(T3), typeof(T4));

            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data1);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data2);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data3);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data4);

            EntitiesStorage.SetEntityArchetype(entity, index.ArchetypeIndex, index.IndexInArchetype);

            return entity;
        }

        public Entity CreateEntity<T1, T2, T3, T4, T5>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5)
            where T1 : unmanaged, IComponentData
            where T2 : unmanaged, IComponentData
            where T3 : unmanaged, IComponentData
            where T4 : unmanaged, IComponentData
            where T5 : unmanaged, IComponentData
        {
            var entity = EntitiesStorage.CreateEntity();

            var index = AddToArchetypeAndGetIndex(entity, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data1);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data2);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data3);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data4);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data5);

            EntitiesStorage.SetEntityArchetype(entity, index.ArchetypeIndex, index.IndexInArchetype);

            return entity;
        }

        public Entity CreateEntity<T1, T2, T3, T4, T5, T6>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6)
            where T1 : unmanaged, IComponentData
            where T2 : unmanaged, IComponentData
            where T3 : unmanaged, IComponentData
            where T4 : unmanaged, IComponentData
            where T5 : unmanaged, IComponentData
            where T6 : unmanaged, IComponentData
        {
            var entity = EntitiesStorage.CreateEntity();

            var index = AddToArchetypeAndGetIndex(entity, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));

            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data1);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data2);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data3);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data4);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data5);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data6);

            EntitiesStorage.SetEntityArchetype(entity, index.ArchetypeIndex, index.IndexInArchetype);

            return entity;
        }

        public Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6, T7 data7)
            where T1 : unmanaged, IComponentData
            where T2 : unmanaged, IComponentData
            where T3 : unmanaged, IComponentData
            where T4 : unmanaged, IComponentData
            where T5 : unmanaged, IComponentData
            where T6 : unmanaged, IComponentData
            where T7 : unmanaged, IComponentData
        {
            var entity = EntitiesStorage.CreateEntity();

            var index = AddToArchetypeAndGetIndex(entity, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));

            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data1);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data2);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data3);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data4);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data5);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data6);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data7);

            EntitiesStorage.SetEntityArchetype(entity, index.ArchetypeIndex, index.IndexInArchetype);

            return entity;
        }

        public Entity CreateEntity<T1, T2, T3, T4, T5, T6, T7, T8>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6, T7 data7, T8 data8)
            where T1 : unmanaged, IComponentData
            where T2 : unmanaged, IComponentData
            where T3 : unmanaged, IComponentData
            where T4 : unmanaged, IComponentData
            where T5 : unmanaged, IComponentData
            where T6 : unmanaged, IComponentData
            where T7 : unmanaged, IComponentData
            where T8 : unmanaged, IComponentData
        {
            var entity = EntitiesStorage.CreateEntity();

            var index = AddToArchetypeAndGetIndex(entity, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));

            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data1);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data2);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data3);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data4);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data5);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data6);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data7);
            DataChunkStorage.SetData(index.IndexInArchetype, index.ArchetypeIndex, data8);

            EntitiesStorage.SetEntityArchetype(entity, index.ArchetypeIndex, index.IndexInArchetype);

            return entity;
        }

        public void DestroyEntity(Entity entity)
        {
            if (!EntitiesStorage.IsEntityAlive(entity))
            {
                return;
            }

            var (archetypeIndex, index) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);

            var movedEntityId = DataChunkStorage.RemoveEntity(index, archetypeIndex);

            if (movedEntityId != -1)
            {
                EntitiesStorage.SetEntityArchetype(EntitiesStorage.GetEntity(movedEntityId), archetypeIndex, index);
            }

            EntitiesStorage.DestroyEntity(entity);
        }

        // TODO: Readonly struct are always copied
        private readonly struct ArchetypeIndexTuple
        {
            public readonly int ArchetypeIndex;

            public readonly int IndexInArchetype;

            public ArchetypeIndexTuple(int archetypeIndex, int indexInArchetype)
            {
                ArchetypeIndex = archetypeIndex;
                IndexInArchetype = indexInArchetype;
            }
        }
    }
}
