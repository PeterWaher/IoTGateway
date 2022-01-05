using System;
using System.IO;
using System.Text;

namespace Waher.Client.WPF.Controls.Script
{
	public class PrintOutput : TextWriter
	{
		private readonly ScriptView scriptView;

		public PrintOutput(ScriptView ScriptView)
		{
			this.scriptView = ScriptView;
		}

		public override Encoding Encoding => Encoding.UTF8;

		public override void Write(string value)
		{
			this.scriptView.Print(value);
		}
	}
}
