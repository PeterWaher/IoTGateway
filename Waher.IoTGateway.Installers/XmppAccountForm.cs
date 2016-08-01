using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Waher.IoTGateway.Installers
{
	public partial class XmppAccountForm : Form
	{
		public XmppAccountForm()
		{
			InitializeComponent();
		}

		private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Link.ToString());
		}

		private void label_Paint(object sender, PaintEventArgs e)
		{
			Label Label = (Label)sender;

			using (SolidBrush Brush = new SolidBrush(Color.MediumPurple))
			{
				e.Graphics.FillEllipse(Brush, e.ClipRectangle);
			}

			using (SolidBrush Brush = new SolidBrush(Color.White))
			{
				SizeF S = e.Graphics.MeasureString(Label.Text, Label.Font);

				e.Graphics.DrawString(Label.Text, Label.Font, Brush,
					e.ClipRectangle.Left + (e.ClipRectangle.Width - S.Width) / 2,
					e.ClipRectangle.Top + (e.ClipRectangle.Height - S.Height) / 2);
			}
		}
	}
}
