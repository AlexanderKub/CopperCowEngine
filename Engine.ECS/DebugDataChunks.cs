using System;
using CopperCowEngine.ECS.DataChunks;
using Newtonsoft.Json;

namespace CopperCowEngine.ECS
{
    public static class DebugDataChunks
    {
        internal class DataRowSnapshot
        {
            public readonly int EntityId;

            public readonly object[] Data;

            public DataRowSnapshot(int entityId, int typesCount)
            {
                EntityId = entityId;
                Data = new object[typesCount];
            }

            public void AddData(int typeIndex, object data)
            {
                Data[typeIndex] = data;
            }
        }

        internal class DataSnapshot
        {
            public readonly int RowCount;

            public readonly int TypesCount;

            public readonly DataRowSnapshot[] Rows;

            public readonly DataChunkArchetype Archetype;

            public DataSnapshot(int rowCount, DataChunkArchetype archetype)
            {
                RowCount = rowCount;
                Rows = new DataRowSnapshot[rowCount];

                Archetype = archetype;
                TypesCount = archetype.ComponentTypes.Length;
            }

            public void AddRow(int index, int entityId)
            {
                Rows[index] = new DataRowSnapshot(entityId, TypesCount);
            }

            public void AddDataToRow(int index, int typeIndex, object data)
            {
                Rows[index].AddData(typeIndex, data);
            }
        }

        internal class Snapshot
        {
            public readonly DataSnapshot[] ArchetypeSnapshots;

            public Snapshot(int archetypesCount)
            {
                ArchetypeSnapshots = new DataSnapshot[archetypesCount];
            }

            public void AddSnapshot(int archetypeIndex, DataSnapshot snapshot)
            {
                ArchetypeSnapshots[archetypeIndex] = snapshot;
            }
        }

        public static void View(EcsContext context)
        {
            var archetypesCount = context.DataChunkStorage.ArchetypesStorage.Count;

            var snapshot = new Snapshot(archetypesCount);

            for (var i = 0; i < archetypesCount; i++)
            {
                var archetype = context.DataChunkStorage.ArchetypesStorage.GetAt(i);
                var chunkChain = context.DataChunkStorage.ChunksStorage[i];

                var lastIndex = chunkChain.Count - 1;
                var rowCount = archetype.ChunkCapacity * lastIndex + chunkChain.Chunks[lastIndex].Count;

                var dataSnapshot = new DataSnapshot(rowCount, archetype);
                for (var j = 0; j < rowCount; j++)
                {
                    var entityId = chunkChain.Chunks[0].GetEntityIdByIndex(j);
                    dataSnapshot.AddRow(j, entityId);

                    for (var k = 0; k < archetype.ComponentTypes.Length; k++)
                    {
                        var data = chunkChain.GetBoxedDataByIndex(j, archetype.ComponentTypes[k]);
                        dataSnapshot.AddDataToRow(j, k, data);
                    }
                }
                snapshot.AddSnapshot(i, dataSnapshot);
            }

            var result = JsonConvert.SerializeObject(snapshot);

            Console.WriteLine(result);
        }
    }
}
