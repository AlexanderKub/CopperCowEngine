using System;
using System.Windows.Forms;
using SharpDX.Windows;

namespace EngineCore
{
    public class EngineRenderForm : RenderForm
    {

        //public event Action<Keys> OnSpecialKeyPressed;
        //protected override bool ProcessKeyEventArgs(ref Message m) {
        //    if (m.Msg != 256) {
        //        return base.ProcessKeyEventArgs(ref m);
        //    }
        //    switch (m.WParam.ToString()) {
        //        case "37":
        //            OnSpecialKeyPressed?.Invoke(Keys.Left);
        //            break;
        //        case "38":
        //            OnSpecialKeyPressed?.Invoke(Keys.Up);
        //            break;
        //        case "39":
        //            OnSpecialKeyPressed?.Invoke(Keys.Right);
        //            break;
        //        case "40":
        //            OnSpecialKeyPressed?.Invoke(Keys.Down);
        //            break;
        //        default:
        //            break;
        //    }
        //    return base.ProcessKeyEventArgs(ref m);
        //}
    }
}