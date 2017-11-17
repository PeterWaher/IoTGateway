using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Client.WPF.Controls.Script
{
	public class PrintOutput : TextWriter
	{
		private ScriptView scriptView;

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
