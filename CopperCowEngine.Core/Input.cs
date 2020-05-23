using System;
using System.Numerics;
using System.Windows.Forms;

namespace CopperCowEngine.Core
{
    public class Input
    {
        public event Action<Keys> OnKeyDown;

        public event Action<Keys> OnKeyUp;

        public event Action<char> OnKeyPress;

        public Vector2 MousePosition { get; private set; }

        internal void TriggerKey(Keys key, bool down)
        {
            if (down)
            {
                OnKeyDown?.Invoke(key);
            }
            else
            {
                OnKeyUp?.Invoke(key);
            }
        }

        internal void TriggerKeyPress(char keyChar)
        {
            OnKeyPress?.Invoke(keyChar);
        }

        internal void SetMousePosition(Vector2 position)
        {
            MousePosition = position;
        }
    }
}
