using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Script.Lab
{
	public class PrintOutput : TextWriter
	{
		private MainWindow mainWindow;

		public PrintOutput(MainWindow MainWindow)
		{
			this.mainWindow = MainWindow;
		}

		public override Encoding Encoding => Encoding.UTF8;

		public override void Write(string value)
		{
			this.mainWindow.Print(value);
		}

		public override void WriteLine(string value)
		{
			this.mainWindow.Print(value);
		}
	}
}
