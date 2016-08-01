using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Waher.IoTGateway.Installers
{
	public class Wizard : TabControl
	{
		public Wizard()
			: base()
		{
		}

		protected override void WndProc(ref Message msg)
		{
			// Hide tabs by trapping the TCM_ADJUSTRECT message

			if (msg.Msg == 0x1328 && !this.DesignMode)
				msg.Result = (IntPtr)1;
			else
				base.WndProc(ref msg);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			// Block Ctrl+Tab and Ctrl+Shift+Tab hotkeys
			if (e.Control && e.KeyCode == Keys.Tab)
				return;

			base.OnKeyDown(e);
		}

	}
}
