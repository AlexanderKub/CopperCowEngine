using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.Utils
{
    public class Benchmark : IDisposable
    {
        private readonly Stopwatch timer = new Stopwatch();
        private readonly string benchmarkName;

        public enum Type
        {
            Milliseconds,
            Ticks,
        }
        private readonly Type benchmarkType;

        public Benchmark(string benchmarkName)
        {
            this.benchmarkName = benchmarkName;
            this.benchmarkType = Type.Milliseconds;
            timer.Start();
        }

        public Benchmark(string benchmarkName, Type type)
        {
            this.benchmarkName = benchmarkName;
            this.benchmarkType = type;
            timer.Start();
        }

        public void Dispose()
        {
            timer.Stop();
            switch (this.benchmarkType)
            {
                case Type.Milliseconds:
                    Debug.Log($"{benchmarkName} {timer.ElapsedMilliseconds}");
                    break;
                case Type.Ticks:
                    Debug.Log($"{benchmarkName} {timer.ElapsedTicks}");
                    break;
                default:
                    break;
            }
        }
    }
}
