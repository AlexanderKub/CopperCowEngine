using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.Utils
{
    public class StatisticsCollector
    {
        public int DrawCallsCount {
            get {
                IsUpdated = false;
                return m_DrawCallsCount;
            }
        }
        public bool IsUpdated { get; private set; }

        private int m_DrawCallsCount;
        private int m_TmpDrawCallsCount;

        internal StatisticsCollector() { }

        internal void ClearDrawcalls()
        {
            if (m_DrawCallsCount != m_TmpDrawCallsCount)
            {
                m_DrawCallsCount = m_TmpDrawCallsCount;
                IsUpdated = true;
            }
            m_TmpDrawCallsCount = 0;
        }

        internal void IncDrawcalls()
        {
            m_TmpDrawCallsCount++;
        }
    }
}
