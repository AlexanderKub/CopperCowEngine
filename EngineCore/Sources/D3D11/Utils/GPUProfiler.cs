using SharpDX.Direct3D11;

namespace EngineCore.D3D11.Utils
{
    public class GPUProfiler
    {
        private Query queryTimeStampDisjoint;
        private Query queryTimeStampStart;
        private Query queryTimeStampEnd;

        public void Initialize(Device device)
        {
            queryTimeStampDisjoint = new Query(device, new QueryDescription() { Type = QueryType.TimestampDisjoint });
            queryTimeStampStart = new Query(device, new QueryDescription() { Type = QueryType.Timestamp });
            queryTimeStampEnd = new Query(device, new QueryDescription() { Type = QueryType.Timestamp });
        }

        public void Begin(DeviceContext context)
        {
            context.Begin(queryTimeStampDisjoint);
            context.Begin(queryTimeStampStart);
        }

        public void End(DeviceContext context)
        {
            context.End(queryTimeStampEnd);
            context.End(queryTimeStampDisjoint);
        }

        public double GetElapsedMilliseconds(DeviceContext context)
        {

            long timestampStart;
            long timestampEnd;
            QueryDataTimestampDisjoint disjointData;

            while (!context.GetData(queryTimeStampStart, AsynchronousFlags.None, out timestampStart)) { }
            while (!context.GetData(queryTimeStampEnd, AsynchronousFlags.None, out timestampEnd)) { }
            while (!context.GetData(queryTimeStampDisjoint, AsynchronousFlags.None, out disjointData)) { }

            if (disjointData.Disjoint) {
                return -1;
            }
            long delta = timestampEnd - timestampStart;
            return (delta * 1000.0) / disjointData.Frequency;
        }
    }
}
