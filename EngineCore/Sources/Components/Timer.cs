using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore
{
    public class Timer
    {
        public float Time
        {
            get;
            protected set;
        }

        public float DeltaTime
        {
            get;
            protected set;
        }
        
        private Stopwatch stopwatch;
        private long lastTimeStamp;
        private long tempTimeStamp;

        public Timer() {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            lastTimeStamp = stopwatch.ElapsedMilliseconds;
            Time = 0;
        }

        public void Update()
        {
            tempTimeStamp = stopwatch.ElapsedMilliseconds;
            DeltaTime = (tempTimeStamp - lastTimeStamp) / 1000.0f;
            Time += DeltaTime;
            lastTimeStamp = tempTimeStamp;
        }
    }
}
