using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.Adapter
{
    public class SuspendDrawingScope : IDisposable
    {
        private const int WM_SETREDRAW = 0x000B;
        private readonly Control _control;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

        public SuspendDrawingScope(Control control)
        {
            _control = control;
            SendMessage(_control.Handle, WM_SETREDRAW, false, 0);
        }

        public void Dispose()
        {
            SendMessage(_control.Handle, WM_SETREDRAW, true, 0);
            _control.Refresh();
        }
    }
}
