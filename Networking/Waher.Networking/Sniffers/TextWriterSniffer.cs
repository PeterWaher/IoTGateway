using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to a text writer.
	/// </summary>
	public class TextWriterSniffer : ISniffer, IDisposable
	{
		protected TextWriter output;
		private BinaryPresentationMethod binaryPresentationMethod;
		private bool disposed = false;

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
		/// Method is called before writing something to the text file.
		/// </summary>
		protected virtual void BeforeWrite()
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method is called after writing something to the text file.
		/// </summary>
		protected virtual void AfterWrite()
		{
			// Do nothing by default.
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
			if (this.disposed)
				return;

			this.BeforeWrite();
			try
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
			finally
			{
				this.AfterWrite();
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
			this.WriteLine("Rx: " + Text);
		}

		private void WriteLine(string s)
		{
			if (this.disposed)
				return;

			this.BeforeWrite();
			try
			{
				this.output.WriteLine(s);
			}
			finally
			{
				this.AfterWrite();
			}
		}

		/// <summary>
		/// <see cref="ISniffer.TransmitText"/>
		/// </summary>
		public virtual void TransmitText(string Text)
		{
			this.WriteLine("Tx: " + Text);
		}

		/// <summary>
		/// <see cref="ISniffer.Information"/>
		/// </summary>
		public virtual void Information(string Comment)
		{
			this.WriteLine("Info: " + Comment);
		}

		/// <summary>
		/// <see cref="ISniffer.Warning"/>
		/// </summary>
		public virtual void Warning(string Warning)
		{
			this.WriteLine("Warning: " + Warning);
		}

		/// <summary>
		/// <see cref="ISniffer.Error"/>
		/// </summary>
		public virtual void Error(string Error)
		{
			this.WriteLine("Error: " + Error);
		}

		/// <summary>
		/// <see cref="ISniffer.Exception"/>
		/// </summary>
		public virtual void Exception(string Exception)
		{
			this.WriteLine("Exception: " + Exception);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			this.disposed = true;
		}
	}
}
