using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Factory = SharpDX.DirectWrite.Factory;
using SharpDX.Mathematics.Interop;

namespace EngineCore.D3D11
{
    internal class ConsoleRenderer
    {
        private D3D11RenderBackend RenderBackend;

        #region Resources
        private SolidColorBrush m_HistoryColorBrush;
        private SolidColorBrush m_ErrorColorBrush;
        private SolidColorBrush m_CommandColorBrush;

        private TextFormat m_TextFormat;
        private TextFormat m_FPSTextFormat;
        private TextLayout m_HistoryTextLayout;
        private TextLayout m_TextLayout;
        private TextLayout m_FPSTextLayout;
        private TextLayout m_CaretTextLayout;

        private RoundedRectangleGeometry m_RectangleGeometry;
        private Brush m_SolidColorBrush;
        #endregion

        private RenderTarget RT2D {
            get {
                return RenderBackend.DisplayRef.RenderTarget2D;
            }
        }

        private Factory FactoryDWrite {
            get {
                return RenderBackend.DisplayRef.FactoryDWrite;
            }
        }

        public void Initialize(D3D11RenderBackend backend)
        {
            RenderBackend = backend;
            if (RT2D == null) {
                return;
            }

            m_HistoryColorBrush = ToDispose(new SolidColorBrush(RT2D, Color.White));
            m_ErrorColorBrush = ToDispose(new SolidColorBrush(RT2D, Color.Red));
            m_CommandColorBrush = ToDispose(new SolidColorBrush(RT2D, Color.Yellow));

            m_TextFormat = ToDispose(new TextFormat(FactoryDWrite, "Roboto", 15) { TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Near });
            m_FPSTextFormat = ToDispose(new TextFormat(FactoryDWrite, "Roboto", 20) { TextAlignment = TextAlignment.Trailing, ParagraphAlignment = ParagraphAlignment.Near });

            m_RectangleGeometry = ToDispose(new RoundedRectangleGeometry(RenderBackend.DisplayRef.Factory2D, new RoundedRectangle()
            {
                RadiusX = 0,
                RadiusY = 0,
                Rect = new RectangleF(0, 0, 500, 1200)
            }));

            m_TextLayout = ToDispose(new TextLayout(FactoryDWrite, "", m_TextFormat, 500, 20));
            m_HistoryTextLayout = ToDispose(new TextLayout(FactoryDWrite, "", m_TextFormat, 500, 200));
            m_CaretTextLayout = ToDispose(new TextLayout(FactoryDWrite, "", m_TextFormat, 500, 20));

            m_SolidColorBrush = ToDispose(new SolidColorBrush(RT2D, new Color(new Vector4(0.7f, 0.7f, 0.7f, 0.6f))));
            m_FPSTextLayout = ToDispose(new TextLayout(FactoryDWrite, "", m_FPSTextFormat, 100, 100));
        }

        public void UpdateTextLayout()
        {
            if (FactoryDWrite.IsDisposed)
            {
                return;
            }

            if (!RenderBackend.EngineRef.GetEngineConsole.IsDirty)
            {
                return;
            }

            RenderBackend.EngineRef.GetEngineConsole.IsDirty = true;

            var console = RenderBackend.EngineRef.GetEngineConsole;

            m_TextLayout?.Dispose();
            m_HistoryTextLayout?.Dispose();
            m_TextLayout = ToDispose(new TextLayout(FactoryDWrite, ">" + console.CurrentCommand, m_TextFormat, 500, 20));
            m_HistoryTextLayout = ToDispose(new TextLayout(FactoryDWrite, console.FullLog, m_TextFormat, 500, 200));
            m_CaretTextLayout?.Dispose();
            m_CaretTextLayout = ToDispose(new TextLayout(FactoryDWrite, ">" + console.CarretLayout, m_TextFormat, 500, 20));
        }

        private float m_DrawCaretTime = 0;
        public void Draw()
        {
            if (!RenderBackend.EngineRef.GetEngineConsole.IsShownConsole) {
                if (!RenderBackend.EngineRef.GetEngineConsole.IsShownStatsMonitor) {
                    return;
                }
                DrawStatsMonitor();
                return;
            }
            UpdateTextLayout();
            DrawStatsMonitor();

            //TODO: rect shell & error coloring
            RT2D.FillGeometry(m_RectangleGeometry, m_SolidColorBrush, null);
            RT2D.DrawTextLayout(new Vector2(0, 0), m_TextLayout, m_CommandColorBrush, DrawTextOptions.None);
            RT2D.DrawTextLayout(new Vector2(0, 20), m_HistoryTextLayout, m_HistoryColorBrush, DrawTextOptions.None);

            m_DrawCaretTime += 1f * RenderBackend.EngineRef.Time.DeltaTime;
            if (m_DrawCaretTime > 0.75)
            {
                int c = (int)m_CaretTextLayout.Metrics.WidthIncludingTrailingWhitespace;
                RT2D.DrawRectangle(new RawRectangleF(c, 3, c, 20), m_CommandColorBrush);
            }
            if (m_DrawCaretTime > 1.2)
            {
                m_DrawCaretTime = 0;
            }
        }

        private void DrawStatsMonitor()
        {
            if (RenderBackend.EngineRef.Profiler.IsUpdated || RenderBackend.EngineRef.Statistics.IsUpdated)
            {
                m_FPSTextLayout?.Dispose();
                string txt = "NO PROFILED";
                txt = RenderBackend.EngineRef.Profiler?.Report() ?? txt;
                txt += $"\nDrawcalls: {RenderBackend.EngineRef.Statistics.DrawCallsCount}";
                m_FPSTextLayout = ToDispose(new TextLayout(FactoryDWrite, txt, m_FPSTextFormat, 300, 100));
            }
            RT2D.DrawTextLayout(
                new Vector2((int)RT2D.Size.Width - 310, 60),
                m_FPSTextLayout, m_CommandColorBrush, DrawTextOptions.None);
        }


        private List<IDisposable> disposables = new List<IDisposable>();
        private T ToDispose<T>(T disposable) where T : IDisposable
        {
            disposables.Add(disposable);
            return disposable;
        }

        public void Dispose()
        {
            foreach (var item in disposables) {
                item?.Dispose();
            }
            disposables.Clear();
            disposables = null;
        }
    }
}
