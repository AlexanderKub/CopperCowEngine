using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Windows.Forms;
using CopperCowEngine.Core;

namespace CopperCowEngine.ECS.Builtin.Singletons
{
    public enum Buttons : byte
    {
        Left,
        Right,
        Up,
        Down,
        LeftShift,
        Esc,
    }

    public enum Axis : byte
    {
        Vertical,
        Horizontal,
        MouseX,
        MouseY,
    }

    public struct InputSingleton : ISingletonComponentData
    {
        public Vector2 MouseOffset;

        private static readonly ReadOnlyDictionary<Keys, Buttons> ButtonsBinding = new ReadOnlyDictionary<Keys, Buttons>(
            new Dictionary<Keys, Buttons>() {
                { Keys.A, Buttons.Left },
                { Keys.D, Buttons.Right },
                { Keys.W, Buttons.Up },
                { Keys.S, Buttons.Down },
                { Keys.ShiftKey, Buttons.LeftShift },
                { Keys.Escape, Buttons.Esc}
            }
        );

        private HashSet<Buttons> _buttonsSet;

        public void Init()
        {
            _buttonsSet = new HashSet<Buttons>();
        }

        public void UpdateMousePosition(Vector2 offset)
        {
            MouseOffset = offset;
        }

        internal void KeyDown(Keys key)
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

        internal void KeyUp(Keys key)
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
