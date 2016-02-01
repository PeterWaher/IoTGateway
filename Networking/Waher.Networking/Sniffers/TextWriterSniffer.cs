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
		private BinaryPresentationMethod binaryPresentationMethod;

		/// <summary>
		/// Outputs sniffed data to a text writer.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public TextWriterSniffer(TextWriter Output, BinaryPresentationMethod BinaryPresentationMethod)
		{
			this.output = Output;
			this.binaryPresentationMethod = BinaryPresentationMethod;
		}

		/// <summary>
		/// <see cref="ISniffer.ReceiveBinary"/>
		/// </summary>
		public virtual void ReceiveBinary(byte[] Data)
		{
			if (Data.Length > 0)
				this.HexOutput(Data, "Rx: ");
		}

		private void HexOutput(byte[] Data, string RowPrefix)
		{
			switch (this.binaryPresentationMethod)
			{
				case BinaryPresentationMethod.Hexadecimal:
					int i = 0;

					foreach (byte b in Data)
					{
						if (i == 0)
							this.output.Write(RowPrefix);
						else
							this.output.Write(' ');

						this.output.Write(b.ToString("X2"));

						i = (i + 1) & 31;
						if (i == 0)
							this.output.WriteLine();
					}

					if (i != 0)
						this.output.WriteLine();
					break;

				case BinaryPresentationMethod.Base64:
					this.output.WriteLine(RowPrefix + System.Convert.ToBase64String(Data));
					break;

				case BinaryPresentationMethod.ByteCount:
					this.output.WriteLine(RowPrefix + "<" + Data.Length.ToString() + " bytes>");
					break;
			}
		}

		/// <summary>
		/// <see cref="ISniffer.TransmitBinary"/>
		/// </summary>
		public virtual void TransmitBinary(byte[] Data)
		{
			if (Data.Length > 0)
				this.HexOutput(Data, "Tx: ");
		}

		/// <summary>
		/// <see cref="ISniffer.ReceiveText"/>
		/// </summary>
		public virtual void ReceiveText(string Text)
		{
			this.output.WriteLine("Rx: " + Text);
		}

		/// <summary>
		/// <see cref="ISniffer.TransmitText"/>
		/// </summary>
		public virtual void TransmitText(string Text)
		{
			this.output.WriteLine("Tx: " + Text);
		}

		/// <summary>
		/// <see cref="ISniffer.Information"/>
		/// </summary>
		public virtual void Information(string Comment)
		{
			this.output.WriteLine("Info: " + Comment);
		}

		/// <summary>
		/// <see cref="ISniffer.Warning"/>
		/// </summary>
		public virtual void Warning(string Warning)
		{
			this.output.WriteLine("Warning: " + Warning);
		}

		/// <summary>
		/// <see cref="ISniffer.Error"/>
		/// </summary>
		public virtual void Error(string Error)
		{
			this.output.WriteLine("Error: " + Error);
		}

		/// <summary>
		/// <see cref="ISniffer.Exception"/>
		/// </summary>
		public virtual void Exception(string Exception)
		{
			this.output.WriteLine("Exception: " + Exception);
		}
	}
}
