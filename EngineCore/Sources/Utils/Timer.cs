using System;
using System.Collections.Generic;
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

        public int FPS
        {
            get;
            protected set;
        }

        private List<DateTime> m_TimeStamps;

        public Timer() {
            Time = 0;
            m_TimeStamps = new List<DateTime>();
        }

        public void Update() {
            m_TimeStamps.Add(DateTime.Now);
            DeltaTime = (float)(m_TimeStamps.Count < 2 ? 0.001 : 
                (m_TimeStamps[m_TimeStamps.Count - 1] - m_TimeStamps[m_TimeStamps.Count - 2]).TotalSeconds);
            Time += DeltaTime;
            if (m_TimeStamps.Count > 9) {
                m_TimeStamps.RemoveAt(0);
            }
            FPS = (int)(m_TimeStamps.Count < 2 ? 0 :
                 (m_TimeStamps.Count / (m_TimeStamps[m_TimeStamps.Count - 1] - m_TimeStamps[0]).TotalSeconds));
        }
    }
}
