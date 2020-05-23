using SharpDX.Direct3D11;

namespace CopperCowEngine.Rendering.D3D11.Utils
{
    public class GpuProfiler
    {
        private Query _queryTimeStampDisjoint;
        private Query _queryTimeStampStart;
        private Query _queryTimeStampEnd;

        private bool _initialized;

        public void Initialize(Device device)
        {
            _queryTimeStampDisjoint = new Query(device, new QueryDescription() { Type = QueryType.TimestampDisjoint });
            _queryTimeStampStart = new Query(device, new QueryDescription() { Type = QueryType.Timestamp });
            _queryTimeStampEnd = new Query(device, new QueryDescription() { Type = QueryType.Timestamp });
            _initialized = true;
        }

        public void Begin(DeviceContext context)
        {
            if (!_initialized)
            {
                Initialize(context.Device);
                _initialized = true;
            }
            context.Begin(_queryTimeStampDisjoint);
            context.Begin(_queryTimeStampStart);
        }

        public void End(DeviceContext context)
        {
            context.End(_queryTimeStampEnd);
            context.End(_queryTimeStampDisjoint);
        }

        public double GetElapsedMilliseconds(DeviceContext context)
        {
            long timestampStart;
            long timestampEnd;
            QueryDataTimestampDisjoint disjointData;

            while (!context.GetData(_queryTimeStampStart, AsynchronousFlags.None, out timestampStart)) { }

            while (!context.GetData(_queryTimeStampEnd, AsynchronousFlags.None, out timestampEnd)) { }

            while (!context.GetData(_queryTimeStampDisjoint, AsynchronousFlags.None, out disjointData)) { }

            if (disjointData.Disjoint)
            {
                return -1;
            }
            var delta = timestampEnd - timestampStart;
            return (delta * 1000.0) / disjointData.Frequency;
        }
    }
}