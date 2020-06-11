using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to a text writer.
	/// </summary>
	public class TextWriterSniffer : SnifferBase, IDisposable
	{
		/// <summary>
		/// Text output writer.
		/// </summary>
		protected TextWriter output;

		private readonly BinaryPresentationMethod binaryPresentationMethod;
		private bool disposed = false;
		private bool includeTimestamp = false;

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
		/// If a timestamp should be included for each record logged.
		/// </summary>
		public bool IncludeTimestamp
		{
			get { return this.includeTimestamp; }
			set { this.includeTimestamp = value; }
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
		public override void ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			if (Data.Length > 0)
				this.HexOutput(Data, this.Prefix(Timestamp, "Rx"));
		}

		private string Prefix(DateTime Timestamp, string Type)
		{
			if (this.includeTimestamp)
				return Type + " (" + Timestamp.ToString("T") + "): ";
			else
				return Type + ": ";
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
		public override void TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			if (Data.Length > 0)
				this.HexOutput(Data, this.Prefix(Timestamp, "Tx"));
		}

		/// <summary>
		/// <see cref="ISniffer.ReceiveText"/>
		/// </summary>
		public override void ReceiveText(DateTime Timestamp, string Text)
		{
			this.WriteLine(this.Prefix(Timestamp, "Rx") + Text);
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
		public override void TransmitText(DateTime Timestamp, string Text)
		{
			this.WriteLine(this.Prefix(Timestamp, "Tx") + Text);
		}

		/// <summary>
		/// <see cref="ISniffer.Information"/>
		/// </summary>
		public override void Information(DateTime Timestamp, string Comment)
		{
			this.WriteLine(this.Prefix(Timestamp, "Info") + Comment);
		}

		/// <summary>
		/// <see cref="ISniffer.Warning"/>
		/// </summary>
		public override void Warning(DateTime Timestamp, string Warning)
		{
			this.WriteLine(this.Prefix(Timestamp, "Warning") + Warning);
		}

		/// <summary>
		/// <see cref="ISniffer.Error"/>
		/// </summary>
		public override void Error(DateTime Timestamp, string Error)
		{
			this.WriteLine(this.Prefix(Timestamp, "Error") + Error);
		}

		/// <summary>
		/// <see cref="ISniffer.Exception"/>
		/// </summary>
		public override void Exception(DateTime Timestamp, string Exception)
		{
			this.WriteLine(this.Prefix(Timestamp, "Exception") + Exception);
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
