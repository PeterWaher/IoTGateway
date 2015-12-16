using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to a text writer.
	/// </summary>
	public class TextWriterSniffer : ISniffer
	{
		protected TextWriter output;

		/// <summary>
		/// Outputs sniffed data to a text writer.
		/// </summary>
		/// <param name="Output">Output</param>
		public TextWriterSniffer(TextWriter Output)
		{
			this.output = Output;
		}

		/// <summary>
		/// <see cref="ISniffer.ReceiveBinary"/>
		/// </summary>
		public void ReceiveBinary(byte[] Data)
		{
			if (Data.Length > 0)
			{
				int i = 0;

				foreach (byte b in Data)
				{
					if (i == 0)
						this.output.Write("Rx: ");
					else
						this.output.Write(' ');

					this.output.Write(b.ToString("X2"));

					i = (i + 1) & 31;
					if (i == 0)
						this.output.WriteLine();
				}

				if (i != 0)
					this.output.WriteLine();
			}
		}

		/// <summary>
		/// <see cref="ISniffer.TransmitBinary"/>
		/// </summary>
		public void TransmitBinary(byte[] Data)
		{
			if (Data.Length > 0)
			{
				int i = 0;

				foreach (byte b in Data)
				{
					if (i == 0)
						this.output.Write("Tx: ");
					else
						this.output.Write(' ');

					this.output.Write(b.ToString("X2"));

					i = (i + 1) & 31;
					if (i == 0)
						this.output.WriteLine();
				}

				if (i != 0)
					this.output.WriteLine();
			}
		}

		/// <summary>
		/// <see cref="ISniffer.ReceiveText"/>
		/// </summary>
		public void ReceiveText(string Text)
		{
			Console.Out.WriteLine("Rx: " + Text);
		}

		/// <summary>
		/// <see cref="ISniffer.TransmitText"/>
		/// </summary>
		public void TransmitText(string Text)
		{
			Console.Out.WriteLine("Tx: " + Text);
		}

		/// <summary>
		/// <see cref="ISniffer.Information"/>
		/// </summary>
		public void Information(string Comment)
		{
			Console.Out.WriteLine("Info: " + Comment);
		}

		/// <summary>
		/// <see cref="ISniffer.Warning"/>
		/// </summary>
		public void Warning(string Warning)
		{
			Console.Out.WriteLine("Warning: " + Warning);
		}

		/// <summary>
		/// <see cref="ISniffer.Error"/>
		/// </summary>
		public void Error(string Error)
		{
			Console.Out.WriteLine("Error: " + Error);
		}

		/// <summary>
		/// <see cref="ISniffer.Exception"/>
		/// </summary>
		public void Exception(string Exception)
		{
			Console.Out.WriteLine("Exception: " + Exception);
		}
	}
}
