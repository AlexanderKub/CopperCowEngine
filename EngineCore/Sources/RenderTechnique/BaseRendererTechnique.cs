namespace EngineCore.RenderTechnique
{
    internal class BaseRendererTechnique
    {
        public virtual void Init() { }
        public virtual void Draw() { }
        public virtual void Resize() { }
        public virtual void Dispose() { }

        public virtual void InitRenderer(Renderer renderer) { }
        public virtual void RenderItem(Renderer renderer) { }

        public virtual void InitLight(Light light) { }
        public virtual void RenderItemLight(Light light) { }

        internal bool m_DebugFlag = false;
        public bool SetDebug() {
            m_DebugFlag = !m_DebugFlag;
            if (!m_DebugFlag) {
                m_DebugIndex = 0;
            }
            OnChangeRender();
            return m_DebugFlag;
        }

        internal int m_DebugIndex = 0;
        public void SetDebugIndex(int index) {
            if (!m_DebugFlag) {
                m_DebugIndex = 0;
                return;
            }
            m_DebugIndex = index;
            OnChangeRender();
        }

        public virtual void OnChangeRender() { }
    }
}
