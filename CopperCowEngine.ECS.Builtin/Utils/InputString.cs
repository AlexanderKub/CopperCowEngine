using System.Text;
using System.Windows.Forms;

namespace CopperCowEngine.ECS.Builtin.Utils
{
    internal class InputString
    {
        private readonly StringBuilder _inputStringBuilder;

        public int CaretIndex { get; private set; }

        public bool IsFocused { get; private set; }

        public InputString()
        {
            _inputStringBuilder = new StringBuilder();
        }

        public void Clear()
        {
            CaretIndex = 0;
            _inputStringBuilder.Clear();
        }

        public string GetString()
        {
            return _inputStringBuilder.ToString();
        }

        public void SetString(string value)
        {
            _inputStringBuilder.Clear();
            _inputStringBuilder.Append(value);
            CaretIndex = value.Length;
        }

        public void SetFocus(bool focus)
        {
            IsFocused = focus;
            if (!focus)
            {
                Clear();
            }
        }

        public void KeyPress(char keyChar)
        {
            if (!IsFocused)
            {
                return;
            }
            
            if (keyChar == (char)Keys.Return || keyChar == (char)Keys.Escape)
            {
                return;
            }

            if (keyChar == (char)Keys.Back)
            {
                if (CaretIndex > 0)
                {
                    CaretIndex--;
                    _inputStringBuilder.Remove(CaretIndex, 1);
                }
                return;
            }

            _inputStringBuilder.Insert(CaretIndex, keyChar);
            CaretIndex++;
        }

        public void HandleInputStringKeys(Keys key)
        {
            if (!IsFocused)
            {
                return;
            }
            
            if (key == Keys.Left)
            {
                if (CaretIndex > 0)
                {
                    CaretIndex--;
                }
                return;
            }

            if (key == Keys.Right)
            {
                if (CaretIndex < _inputStringBuilder.Length)
                {
                    CaretIndex++;
                }
                return;
            }
        }
    }
}
