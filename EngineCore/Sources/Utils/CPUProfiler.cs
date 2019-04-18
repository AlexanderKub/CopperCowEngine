using System;
using System.Diagnostics;

namespace EngineCore.Utils
{
    public class CPUProfiler
    {
        private bool m_CanReadCPU;
        private PerformanceCounter m_Counter;

        private TimeSpan m_LastSampleTime;
        private int m_FramesCount = 0;

        private long m_CpuUsage;
        private float m_FrameTime;

        public int Value { get { return m_CanReadCPU ? (int)m_CpuUsage : 0; } }
        public float FrameTime { get { return m_CanReadCPU ? (float)m_FrameTime : 0; } }
        public bool IsUpdated { get; private set; }

        public CPUProfiler Initialize()
        {
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

            return this;
        }

        public void Shutdown() {
            if (m_CanReadCPU) {
                m_Counter.Close();
            }
        }

        public void Frame() {
            if (m_CanReadCPU) {
                double msElapsed = (DateTime.Now.TimeOfDay - m_LastSampleTime).TotalMilliseconds;
                m_LastECSSampleTime = DateTime.Now.TimeOfDay;
                m_FramesCount++;

                if (msElapsed >= 1000)
                {
                    IsUpdated = true;
                    m_FrameTime = (float)msElapsed / m_FramesCount;
                    m_LastSampleTime = DateTime.Now.TimeOfDay;
                    m_CpuUsage = (int)m_Counter.NextValue();
                    m_FramesCount = 0;

                    ECSAverageTime = m_ECSAverage / m_ECSTicks;
                    m_ECSAverage = 0;
                    m_ECSTicks = 0;
                }
            }
        }

        public double ECSAverageTime;
        private TimeSpan m_LastECSSampleTime;
        private double m_ECSAverage = 0;
        private int m_ECSTicks = 0;
        public void ECS()
        {
            if (m_CanReadCPU)
            {
                m_ECSAverage += (DateTime.Now.TimeOfDay - m_LastECSSampleTime).TotalMilliseconds;
                m_ECSTicks++;
            }
        }

        public string Report()
        {
            IsUpdated = false;
            return $"CPU time: {FrameTime:N1}ms {(int)(1f / FrameTime * 1000f)} fps \nCPU load: {Value.ToString() ?? "NONE"}%" +
                $"\nECS: {ECSAverageTime:N1}ms";
        }
    }
}
