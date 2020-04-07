using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopperCowEngine.Core
{
    public static class Time
    {
        private static Stopwatch _stopwatch;

        private static long _lastTimeStamp;

        private static long _tempTimeStamp;

        public static float Current { get; private set; }

        public static float Delta { get; private set; }

        internal static void Start()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _lastTimeStamp = _stopwatch.ElapsedMilliseconds;
            Current = 0;
        }

        internal static void Update()
        {
            _tempTimeStamp = _stopwatch.ElapsedMilliseconds;
            Delta = (_tempTimeStamp - _lastTimeStamp) / 1000.0f;
            Current += Delta;
            _lastTimeStamp = _tempTimeStamp;
        }

        internal static void Stop()
        {
            _stopwatch.Stop();
        }
    }
}
