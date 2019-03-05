using System;
using System.Diagnostics;

namespace EngineCore.Utils
{
    public class CPUProfiler
    {
        private bool m_CanReadCPU;
        private PerformanceCounter m_Counter;
        private TimeSpan m_LastSampleTime;
        private long m_CpuUsage;

        public int Value { get { return m_CanReadCPU ? (int)m_CpuUsage : 0; } }

        public void Initialize() {
            m_CanReadCPU = true;

            try {
                m_Counter = new PerformanceCounter();
                m_Counter.CategoryName = "Processor";
                m_Counter.CounterName = "% Processor Time";
                m_Counter.InstanceName = "_Total";

                m_LastSampleTime = DateTime.Now.TimeOfDay;

                m_CpuUsage = 0;
            } catch {
                m_CanReadCPU = false;
            }
        }

        public void Shutdown() {
            if (m_CanReadCPU) {
                m_Counter.Close();
            }
        }

        public void Frame() {
            if (m_CanReadCPU) {
                int secondsElapsed = (DateTime.Now.TimeOfDay - m_LastSampleTime).Seconds;

                if (secondsElapsed >= 1) {
                    m_LastSampleTime = DateTime.Now.TimeOfDay;
                    m_CpuUsage = (int)m_Counter.NextValue();
                }
            }
        }
    }
}
