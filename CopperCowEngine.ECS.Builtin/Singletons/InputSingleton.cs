using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;

namespace CopperCowEngine.ECS.Builtin.Singletons
{
    public enum Buttons
    {
        Left,
        Right,
        Up,
        Down,
        LeftShift,
    }

    public enum Axis
    {
        Vertical,
        Horizontal,
        MouseX,
        MouseY,
    }

    public struct InputSingleton : ISingletonComponentData
    {
        public float MouseXOffset;
        public float MouseYOffset;

        private static readonly ReadOnlyDictionary<Keys, Buttons> ButtonsBinding = new ReadOnlyDictionary<Keys, Buttons>(
            new Dictionary<Keys, Buttons>() {
                { Keys.A, Buttons.Left },
                { Keys.D, Buttons.Right },
                { Keys.W, Buttons.Up },
                { Keys.S, Buttons.Down },
                { Keys.ShiftKey, Buttons.LeftShift },
            }
        );

        private HashSet<Buttons> _buttonsSet;

        public void Init()
        {
            _buttonsSet = new HashSet<Buttons>();
        }

        public void UpdateMousePosition(Vector2 offset)
        {
            MouseXOffset = offset.X;
            MouseYOffset = offset.Y;
        }

        public void KeyDown(Keys key)
        {
            if (!ButtonsBinding.ContainsKey(key))
            {
                return;
            }

            var b = ButtonsBinding[key];

            if (!_buttonsSet.Contains(b))
            {
                _buttonsSet.Add(b);
            }
        }

        public void KeyUp(Keys key)
        {
            if (!ButtonsBinding.ContainsKey(key))
            {
                return;
            }

            var b = ButtonsBinding[key];

            if (_buttonsSet.Contains(b))
            {
                _buttonsSet.Remove(b);
            }
        }

        public bool IsButtonDown(Buttons button)
        {
            return _buttonsSet.Contains(button);
        }

        public void Reset()
        {
            _buttonsSet.Clear();
        }
    }
}
