using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EngineCore.Utils
{
    internal sealed class EngineConsole
    {
        private readonly Engine EngineRef;
        public bool IsShownConsole { get; private set; }
        public bool IsShownStatsMonitor { get; private set; }

        public EngineConsole(Engine engine)
        {
            EngineRef = engine;
            CurrentCommand = "";
            m_CommandsHistory = new List<string>();
            m_Lines = new List<string>();
        }

        private bool openPress = true;
        public bool Toggle()
        {
            if (IsShownConsole = !IsShownConsole)
            {
                openPress = true;
                OnKeyDown += m_OnKeyDown;
                EngineRef.OnCharPressed += OnCharPressed;
            }
            else
            {
                OnKeyDown -= m_OnKeyDown;
                EngineRef.OnCharPressed -= OnCharPressed;
            }
            return IsShownConsole;
            // TODO: add/remove keyboard listeners
        }

        public void ToggleStatsMonitor()
        {
            IsShownStatsMonitor = !IsShownStatsMonitor;
        }

        #region Inputs
        public Action<Keys> OnKeyDown;
        private void m_OnKeyDown(Keys key)
        {
            if (key == Keys.Up)
            {
                if (m_HistoryIndex >= m_CommandsHistory.Count)
                {
                    return;
                }
                m_HistoryIndex++;
                CurrentCommand = m_CommandsHistory[m_CommandsHistory.Count - m_HistoryIndex];
                m_CaretIndex = CurrentCommand.Length;
                IsDirty = true;
                return;
            }

            if (key == Keys.Down)
            {
                if (m_HistoryIndex <= 1)
                {
                    return;
                }
                m_HistoryIndex--;
                CurrentCommand = m_CommandsHistory[m_CommandsHistory.Count - m_HistoryIndex];
                m_CaretIndex = CurrentCommand.Length;
                IsDirty = true;
                return;
            }

            if (key == Keys.Left)
            {
                if (m_CaretIndex == 0)
                {
                    return;
                }
                m_CaretIndex--;
                IsDirty = true;
            }

            if (key == Keys.Right)
            {
                if (m_CaretIndex == CurrentCommand.Length)
                {
                    return;
                }
                m_CaretIndex++;
                IsDirty = true;
            }
        }

        private void OnCharPressed(char key)
        {
            if (key == (char)Keys.Return)
            {
                SubmitLine();
                return;
            }

            if (key == (char)Keys.Back)
            {
                BackLine();
                return;
            }
            if (openPress)
            {
                openPress = false;
                return;
            }
            AddChar(key);
        }

        public void BackLine()
        {
            if (CurrentCommand.Length <= 0)
            {
                return;
            }
            if (m_CaretIndex == 0)
            {
                return;
            }
            m_CaretIndex--;
            CurrentCommand = CurrentCommand.Remove(m_CaretIndex, 1);
            IsDirty = true;
        }

        public void SubmitLine()
        {
            if (CurrentCommand.Length <= 0)
            {
                return;
            }

            string res = "Not";
            
            if (CurrentCommand == "fps") {
                ToggleStatsMonitor();
                res = "Ok";
            } else {
                res = EngineRef.ScriptEngineRef.ExecuteScriptLine(CurrentCommand);
            }

            if (res == "Ok")
            {
                m_CommandsHistory.Add(CurrentCommand);
                CurrentCommand = "";
                m_HistoryIndex = 0;
                m_CaretIndex = 0;
            }
            else
            {
                m_Lines.Add(res);
            }
            IsDirty = true;
        }

        internal int m_CaretIndex { get; private set; }
        private int m_HistoryIndex;
        internal string CurrentCommand { get; private set; }
        internal string CarretLayout {
            get {
                int m = Math.Min(m_CaretIndex, CurrentCommand.Length);
                m = m > 0 ? m : 0;
                return CurrentCommand.Substring(0, m);
            }
        }

        private List<string> m_CommandsHistory;
        private List<string> m_Lines;

        public string FullLog {
            get {
                string result = "";
                if (m_Lines.Count > 0)
                {
                    for (int i = m_Lines.Count - 1; i >= 0; i--)
                    {
                        result += m_Lines[i] + "\n";
                    }
                }
                return result;
            }
        }

        public void LogLine(string line)
        {
            m_Lines.Add(line);
            if (m_Lines.Count > 40)
            {
                m_Lines.RemoveAt(0);
            }
            IsDirty = true;
        }

        internal bool IsDirty = true;

        public void AddChar(char ch)
        {
            CurrentCommand = CurrentCommand.Insert(m_CaretIndex, ch.ToString());
            m_CaretIndex++;
            IsDirty = true;
        }
        #endregion
    }
}
