using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Keys = System.Windows.Forms.Keys;

namespace EngineCore.ECS.Components
{
    public sealed class SingletonInput : ISingletonEntityComponent
    {
        public enum Buttons
        {
            LEFT,
            RIGHT,
            UP,
            DOWN,
            LSHIFT,
        }

        public enum Axis
        {
            Vertical,
            Horizontal,
            MouseX,
            MouseY,
        }

        private HashSet<Buttons> IsDownedButtons;
        private readonly ReadOnlyDictionary<Keys, Buttons> ButtonsBinding = new ReadOnlyDictionary<Keys, Buttons>(
            new Dictionary<Keys, Buttons>() {
                { Keys.A, Buttons.LEFT },
                { Keys.D, Buttons.RIGHT },
                { Keys.W, Buttons.UP },
                { Keys.S, Buttons.DOWN },
                { Keys.ShiftKey, Buttons.LSHIFT },
            }
        );

        public SingletonInput()
        {
            IsDownedButtons = new HashSet<Buttons>();
        }

        public float MouseXOffset = 0.0f;
        public float MouseYOffset = 0.0f;
        internal void UpdateMousePosition(Vector2 Offset)
        {
            MouseXOffset = Offset.X;
            MouseYOffset = Offset.Y;
        }

        internal void AddButtonFromKeyDown(Keys Key)
        {
            if (!ButtonsBinding.ContainsKey(Key))
            {
                return;
            }
            Buttons b = ButtonsBinding[Key];
            if (IsDownedButtons.Contains(b))
            {
                return;
            }
            IsDownedButtons.Add(b);
        }

        internal void RemoveButtonFromKeyUp(Keys Key)
        {
            if (!ButtonsBinding.ContainsKey(Key))
            {
                return;
            }
            Buttons b = ButtonsBinding[Key];
            if (!IsDownedButtons.Contains(b))
            {
                return;
            }
            IsDownedButtons.Remove(b);
        }

        internal void ClearInputs()
        {
            MouseXOffset = MouseYOffset = 0;
            IsDownedButtons.Clear();
        }
        public bool IsButtonDown(Buttons button)
        {
            return IsDownedButtons.Contains(button);
        }
    }
}
