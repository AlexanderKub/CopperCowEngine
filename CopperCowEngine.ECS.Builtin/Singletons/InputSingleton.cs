using CopperCowEngine.ECS.Builtin.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using System.Windows.Forms;

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
        Tilde,
        L,
        Submit,
        ArrowUp,
        ArrowDown,
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
                { Keys.Escape, Buttons.Esc },
                { Keys.Oemtilde, Buttons.Tilde },
                { Keys.L, Buttons.L },
                { Keys.Enter, Buttons.Submit },
                { Keys.Up, Buttons.ArrowUp },
                { Keys.Down, Buttons.ArrowDown },
            }
        );

        private HashSet<Buttons> _buttonsSet;
        private HashSet<Buttons> _pressedSet;
        private HashSet<Buttons> _pressedUpSet;

        internal InputString InputStringHolder;

        public void Init()
        {
            _buttonsSet = new HashSet<Buttons>();
            _pressedSet = new HashSet<Buttons>();
            _pressedUpSet = new HashSet<Buttons>();
            InputStringHolder = new InputString();
        }

        // TODO: refactoring
        public void UpdateMousePosition(Vector2 offset)
        {
            MouseOffset = offset;
        }

        public bool IsButtonDown(Buttons button)
        {
            return _buttonsSet.Contains(button);
        }

        public bool IsButtonPressed(Buttons button)
        {
            var b = _pressedSet.Contains(button);
            _pressedSet.Remove(button);
            return b;
        }

        public bool IsButtonPressedUp(Buttons button)
        {
            var b = _pressedUpSet.Contains(button);
            _pressedUpSet.Remove(button);
            return b;
        }

        internal void KeyDown(Keys key)
        {
            InputStringHolder.HandleInputStringKeys(key);
            if (!ButtonsBinding.ContainsKey(key))
            {
                return;
            }
            var b = ButtonsBinding[key];

            if (_buttonsSet.Contains(b))
            {
                return;
            }
            _pressedUpSet.Remove(b);
            _buttonsSet.Add(b);
            _pressedSet.Add(b);
        }

        internal void KeyUp(Keys key)
        {
            if (!ButtonsBinding.ContainsKey(key))
            {
                return;
            }
            var b = ButtonsBinding[key];

            if (!_buttonsSet.Contains(b))
            {
                return;
            }
            _buttonsSet.Remove(b);
            _pressedSet.Remove(b);
            _pressedUpSet.Add(b);
        }

        internal void KeyPress(char keyChar)
        {
            InputStringHolder.KeyPress(keyChar);
        }
    }
}
