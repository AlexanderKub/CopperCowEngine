using System;
using System.Numerics;
using System.Windows.Forms;
using SharpDX.RawInput;
using SharpDX.Multimedia;

namespace CopperCowEngine.Rendering.D3D11
{
    public partial class D3D11RenderBackend
    {
        public override event Action<Vector2> OnMousePositionChange;

        public override event Action<Keys, bool> OnInputKey;

        private KeyEventHandler _keyDownEventHandler;

        private KeyEventHandler _keyUpEventHandler;

        private void RegisterInputHandling()
        {
            RegisterInputDevice();

            _keyDownEventHandler = (o, args) => 
            {
                OnInputKey?.Invoke(args.KeyCode, true);
            };
            Surface.KeyDown += _keyDownEventHandler;

            _keyUpEventHandler = (o, args) => 
            {
                OnInputKey?.Invoke(args.KeyCode, false);
            };
            Surface.KeyUp += _keyUpEventHandler;
        }

        private void UnRegisterInputHandling()
        {
            SharpDX.RawInput.Device.MouseInput -= Device_MouseInput;
            if (!IsSingleFormMode)
            {
                return;
            }
            Surface.KeyDown -= _keyDownEventHandler;
            Surface.KeyUp -= _keyUpEventHandler;
        }

        private void RegisterInputDevice()
        {
            SharpDX.RawInput.Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.None);
            SharpDX.RawInput.Device.MouseInput += Device_MouseInput;
        }

        private void Device_MouseInput(object sender, MouseInputEventArgs e) 
        {
            OnMousePositionChange?.Invoke(new Vector2(e.X, e.Y));
        }
    }
}
