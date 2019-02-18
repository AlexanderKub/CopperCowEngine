using System;
using System.Windows.Forms;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.DirectWrite;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Factory = SharpDX.Direct2D1.Factory;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;

namespace EngineCore {
    class UIConsole {
        public SolidColorBrush HistoryColorBrush { get; private set; }
        public SolidColorBrush ErrorColorBrush { get; private set; }
        public SolidColorBrush CommandColorBrush { get; private set; }

        public TextFormat TextFormat { get; private set; }
        public TextFormat FPSTextFormat { get; private set; }
        public TextLayout HistoryTextLayout { get; private set; }
        public TextLayout TextLayout { get; private set; }
        public TextLayout FPSTextLayout { get; private set; }
        private TextLayout CaretTextLayout;
        public bool IsShowConsole = false;
        public bool IsShowFPSCounter = false;

        private string m_CurrentCommand;
        private List<string> m_Lines;
        private List<string> m_CommandsHistory;
        private int m_HistoryIndex;
        private int m_CaretIndex;
        private string m_ResultString;
        private RoundedRectangleGeometry m_RectangleGeometry;
        private Brush m_SolidColorBrush;

        private Display DisplayRef
        {
            get {
                return Engine.Instance.DisplayRef;
            }
        }

        public void Init() {
            HistoryColorBrush = new SolidColorBrush(DisplayRef.RenderTarget2D, Color.White);
            ErrorColorBrush = new SolidColorBrush(DisplayRef.RenderTarget2D, Color.Red);
            CommandColorBrush = new SolidColorBrush(DisplayRef.RenderTarget2D, Color.Yellow);

            TextFormat = new TextFormat(DisplayRef.FactoryDWrite, "Roboto", 15) { TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Near };
            FPSTextFormat = new TextFormat(DisplayRef.FactoryDWrite, "Roboto", 40) { TextAlignment = TextAlignment.Trailing, ParagraphAlignment = ParagraphAlignment.Far };


            m_Lines = new List<string> {
                "",
            };
            m_CurrentCommand = "";
            m_CaretIndex = 0;
            m_HistoryIndex = 0;
            m_CommandsHistory = new List<string>();
            UpdateTextLayout();

            m_RectangleGeometry = new RoundedRectangleGeometry(DisplayRef.Factory2D, new RoundedRectangle() {
                RadiusX = 0,
                RadiusY = 0,
                Rect = new RectangleF(0, 0, 500, 1200)
            });
            m_SolidColorBrush = new SolidColorBrush(DisplayRef.RenderTarget2D, new Color(new Vector4(0.7f, 0.7f, 0.7f, 0.6f)));
            FPSTextLayout = new TextLayout(DisplayRef.FactoryDWrite, "" + m_FPS, FPSTextFormat, 100, 100);
        }

        float m_FramesTime = 0;
        int m_FrameCount = 0;
        int m_FPS = 60;
        float m_FT = 0;
        public void Update() {
            m_FrameCount++;
            m_FramesTime += Engine.Instance.Time.DeltaTime;
            if (m_FrameCount > 30) {
                m_FPS = (m_FPS + (int)(1f / (m_FramesTime / m_FrameCount))) / 2;
                m_FT = (float)Math.Round(Engine.Instance.Time.DeltaTime * 10000.0) / 10f;
                m_FrameCount = 0;
                m_FramesTime = 0;
                FPSTextLayout?.Dispose();

                string txt = $"CPU: {m_FT:N1}ms\nGPU: {Engine.Instance.gpuFrameTime:N1}ms\nFPS: {m_FPS}";
                FPSTextLayout = new TextLayout(DisplayRef.FactoryDWrite, txt, FPSTextFormat, 300, 100);
            }
        }

        public void OnCharPressed(char key) {
            if (key == '~' || key == '`') {
                IsShowConsole = !IsShowConsole;
                return;
            }

            if (!IsShowConsole) {
                return;
            }

            if (key == (char)Keys.Return) {
                SubmitLine();
                return;
            }

            if (key == (char)Keys.Back) {
                BackLine();
                return;
            }

            AddChar(key);
        }

        public void OnSpecialKeyPressed(Keys key) {
            if (!IsShowConsole) {
                return;
            }

            if (key == Keys.Up) {
                if (m_HistoryIndex >= m_CommandsHistory.Count) {
                    return;
                }
                m_HistoryIndex++;
                m_CurrentCommand = m_CommandsHistory[m_CommandsHistory.Count - m_HistoryIndex];
                m_CaretIndex = m_CurrentCommand.Length;
                UpdateTextLayout();
                return;
            }

            if (key == Keys.Down) {
                if (m_HistoryIndex <= 1) {
                    return;
                }
                m_HistoryIndex--;
                m_CurrentCommand = m_CommandsHistory[m_CommandsHistory.Count - m_HistoryIndex];
                m_CaretIndex = m_CurrentCommand.Length;
                UpdateTextLayout();
                return;
            }

            if (key == Keys.Left) {
                if (m_CaretIndex == 0) {
                    return;
                }
                m_CaretIndex--;
                UpdateCaretLayout();
            }

            if (key == Keys.Right) {
                if (m_CaretIndex == m_CurrentCommand.Length) {
                    return;
                }
                m_CaretIndex++;
                UpdateCaretLayout();
            }
        }

        public void AddChar(char ch) {
            m_CurrentCommand = m_CurrentCommand.Insert(m_CaretIndex, ch.ToString());
            m_CaretIndex++;
            UpdateTextLayout();
        }

        public void BackLine() {
            if (m_CurrentCommand.Length <= 0) {
                return;
            }
            if (m_CaretIndex == 0) {
                return;
            }
            m_CaretIndex--;
            m_CurrentCommand = m_CurrentCommand.Remove(m_CaretIndex, 1);
            UpdateTextLayout();
        }

        public void SubmitLine() {
            if (m_CurrentCommand.Length <= 0) {
                return;
            }
            string res = Engine.Instance.ScriptEngineInstance.ExecuteScriptLine(m_CurrentCommand);
            if (res == "Ok") {
                m_CommandsHistory.Add(m_CurrentCommand);
                m_CurrentCommand = "";
                m_HistoryIndex = 0;
                m_CaretIndex = 0;
            } else {
                m_Lines.Add(res);
            }
            UpdateTextLayout();
        }

        public void LogLine(string line) {
            m_Lines.Add(line);
            UpdateTextLayout();
        }

        private float m_DrawCaretTime = 0;
        public void Draw() {
            if (IsShowFPSCounter || IsShowConsole) {
                DisplayRef.RenderTarget2D.DrawTextLayout(
                    new Vector2((int)DisplayRef.RenderTarget2D.Size.Width - 310, 60),
                    FPSTextLayout, CommandColorBrush, DrawTextOptions.None);
            }
            if (!IsShowConsole) {
                return;
            }
            //TODO: rect shell & error coloring
            DisplayRef.RenderTarget2D.FillGeometry(m_RectangleGeometry, m_SolidColorBrush, null);
            DisplayRef.RenderTarget2D.DrawTextLayout(new Vector2(0, 0), TextLayout, CommandColorBrush, DrawTextOptions.None);
            DisplayRef.RenderTarget2D.DrawTextLayout(new Vector2(0, 20), HistoryTextLayout, HistoryColorBrush, DrawTextOptions.None);
            m_DrawCaretTime += Engine.Instance.Time.DeltaTime;
            if (m_DrawCaretTime > 0.75) {
                int c = (int)CaretTextLayout.Metrics.WidthIncludingTrailingWhitespace;
                DisplayRef.RenderTarget2D.DrawRectangle(new SharpDX.Mathematics.Interop.RawRectangleF(c, 3, c, 20), CommandColorBrush);
            }
            if (m_DrawCaretTime > 1.2) {
                m_DrawCaretTime = 0;
            }
        }

        public void UpdateTextLayout() {
            m_ResultString = "";
            if (m_Lines.Count > 0) {
                for (int i = m_Lines.Count - 1; i >= 0; i--) {
                    m_ResultString += m_Lines[i] + "\n";
                }
            }

            TextLayout?.Dispose();
            CaretTextLayout?.Dispose();
            HistoryTextLayout?.Dispose();

            if (DisplayRef.FactoryDWrite.IsDisposed) {
                return;
            }

            TextLayout = new TextLayout(DisplayRef.FactoryDWrite, ">" + m_CurrentCommand, TextFormat, 500, 20);
            HistoryTextLayout = new TextLayout(DisplayRef.FactoryDWrite, m_ResultString, TextFormat, 500, 200);
            UpdateCaretLayout();
        }

        private void UpdateCaretLayout() {
            int m = Math.Min(m_CaretIndex, m_CurrentCommand.Length);
            m = m > 0 ? m : 0;
            CaretTextLayout = new TextLayout(DisplayRef.FactoryDWrite, ">" + m_CurrentCommand.Substring(0, m), TextFormat, 500, 20);
        }

        public void Dispose() {
            m_RectangleGeometry.Dispose();
            HistoryColorBrush.Dispose();

            ErrorColorBrush.Dispose();
            CommandColorBrush.Dispose();
            m_SolidColorBrush.Dispose();
            TextFormat.Dispose();
            TextFormat.Dispose();

            FPSTextLayout.Dispose();
            TextLayout.Dispose();
            CaretTextLayout.Dispose();
            HistoryTextLayout.Dispose();
        }

        public void ToggleFPSCounter() {
            IsShowFPSCounter = !IsShowFPSCounter;
        }
    }
}
