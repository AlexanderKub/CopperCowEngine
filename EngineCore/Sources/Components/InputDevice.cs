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
        
		public Vector2 MousePositionLocal	{ get; private set; }
		public Vector2 MouseOffset			{ get; private set; }
        public Vector2 MouseDelta { get; private set; }

        public struct MouseMoveEventArgs
		{
			public Vector2 Position;
			public Vector2 Offset;
		}
        
        private event Action<Keys, bool> KeyEvent;
        private event Action<MouseMoveEventArgs> MouseEvent;

        public InputDevice(Engine engine, Action<Keys, bool> EngineKeysCallback, Action<MouseMoveEventArgs> EngineMouseCallback)
		{
            EngineRef = engine;
            KeyEvent = EngineKeysCallback;
            MouseEvent = EngineMouseCallback;

            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.None);
			Device.MouseInput += Device_MouseInput;

			Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None);
			Device.KeyboardInput += Device_KeyboardInput;

        }

		private void Device_KeyboardInput(object sender, KeyboardInputEventArgs e) {
            bool Break	= e.ScanCodeFlags.HasFlag(ScanCodeFlags.Break);

			if (Break) {
                KeyEvent?.Invoke(e.Key, false);
			} else {
                KeyEvent?.Invoke(e.Key, true);
            }
        }

        private void Device_MouseInput(object sender, MouseInputEventArgs e) {
            //var p = EngineRef.MousePosition;
            //MousePositionLocal = new Vector2(p.X, p.Y);
			MouseOffset = new Vector2(e.X, e.Y);
            MouseEvent?.Invoke(new MouseMoveEventArgs() { Position = Vector2.Zero, Offset = MouseOffset });
        }
    }
}
