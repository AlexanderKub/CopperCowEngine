using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.RawInput;
using SharpDX.Multimedia;
using System.Windows.Forms;

namespace EngineCore
{
	public class InputDevice
	{
		public Engine EngineRef;

		public HashSet<Keys> PressedKeys = new HashSet<Keys>();

		public Vector2 MousePositionLocal	{ get; private set; }
		public Vector2 MouseOffset			{ get; private set; }
        private Vector2 m_OldMouseDelta = Vector2.Zero;
        public Vector2 MouseDelta { get; private set; }

        public struct MouseMoveEventArgs
		{
			public Vector2 Position;
			public Vector2 Offset;
		}

		public event Action<MouseMoveEventArgs> MouseMove;
        public event Action<Keys> KeyPressed;
        public event Action<char> KeyCharInput;

        public InputDevice(Engine engine)
		{
            EngineRef = engine;

			Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.None);
			Device.MouseInput += Device_MouseInput;

			Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None);
			Device.KeyboardInput += Device_KeyboardInput;
		}

		private void Device_KeyboardInput(object sender, KeyboardInputEventArgs e) {
            if (EngineRef.IsInputTextFieldFocus) {
                return;
            }

            bool Break	= e.ScanCodeFlags.HasFlag(ScanCodeFlags.Break);

			if (Break) {
				if (PressedKeys.Contains(e.Key)) RemovePressedKey(e.Key);
			} else {
				if (!PressedKeys.Contains(e.Key)) AddPressedKey(e.Key);
			}
        }

        public void WpfKeyboardInputReset() {
            PressedKeys.Clear();
        }

        public void WpfKeyboardInput(bool Break, Keys Key) {
            if (Break) {
                if (PressedKeys.Contains(Key)) RemovePressedKey(Key);
            } else {
                if (!PressedKeys.Contains(Key)) AddPressedKey(Key);
            }
        }


        private void Device_MouseInput(object sender, MouseInputEventArgs e) {
            if (EngineRef.IsInputTextFieldFocus) {
                return;
            }

            var p = EngineRef.MousePosition;

            MousePositionLocal = new Vector2(p.X, p.Y);
			MouseOffset = new Vector2(e.X, e.Y);

            MouseMove?.Invoke(new MouseMoveEventArgs() { Position = MousePositionLocal, Offset = MouseOffset });
        }
        
		void AddPressedKey(Keys key) 
            {
            KeyPressed?.Invoke(key);
            PressedKeys.Add(key);
		}
        
		void RemovePressedKey(Keys key)
		{
			if (PressedKeys.Contains(key))
			{
				PressedKeys.Remove(key);
			}
		}


		public bool IsKeyDown(Keys key, bool ignoreInputMode = true)
		{
            if (EngineRef.IsInputTextFieldFocus) {
                return false ;
            }
            return (PressedKeys.Contains(key));
		}


		public bool IsKeyUp(Keys key) {
            if (EngineRef.IsInputTextFieldFocus) {
                return true;
            }
            return !IsKeyDown(key);
		}

        public void Device_FormKeyPress(object sender, KeyPressEventArgs e) {
            KeyCharInput(e.KeyChar);
        }

        public void OnKeyCharPressWpf(char keyChar) {
            KeyCharInput(keyChar);
        }
    }
}
