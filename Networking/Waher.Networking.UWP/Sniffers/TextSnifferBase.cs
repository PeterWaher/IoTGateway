using System;
using System.Text;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Abstract base class for plain text-based sniffers.
	/// </summary>
	public abstract class TextSnifferBase : SnifferBase
	{
		private readonly BinaryPresentationMethod binaryPresentationMethod;
		private bool includeTimestamp = false;

		/// <summary>
		/// Outputs sniffed data to a text writer.
		/// </summary>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public TextSnifferBase(BinaryPresentationMethod BinaryPresentationMethod)
		{
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
		/// <see cref="ISniffer.ReceiveBinary(DateTime, byte[])"/>
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
			this.BeforeWrite();
			try
			{
				switch (this.binaryPresentationMethod)
				{
					case BinaryPresentationMethod.Hexadecimal:
						StringBuilder sb = new StringBuilder();
						int i = 0;

						foreach (byte b in Data)
						{
							if (i == 0)
								sb.Append(RowPrefix);
							else
								sb.Append(' ');

							sb.Append(b.ToString("X2"));

							i = (i + 1) & 31;
							if (i == 0)
							{
								this.WriteLine(sb.ToString());
								sb.Clear();
							}
						}

						if (i != 0)
							this.WriteLine(sb.ToString());
						break;

					case BinaryPresentationMethod.Base64:
						this.WriteLine(RowPrefix + Convert.ToBase64String(Data));
						break;

					case BinaryPresentationMethod.ByteCount:
						this.WriteLine(RowPrefix + "<" + Data.Length.ToString() + " bytes>");
						break;
				}
			}
			finally
			{
				this.AfterWrite();
			}
		}

		/// <summary>
		/// <see cref="ISniffer.TransmitBinary(DateTime, byte[])"/>
		/// </summary>
		public override void TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			if (Data.Length > 0)
				this.HexOutput(Data, this.Prefix(Timestamp, "Tx"));
		}

		/// <summary>
		/// <see cref="ISniffer.ReceiveText(DateTime, String)"/>
		/// </summary>
		public override void ReceiveText(DateTime Timestamp, string Text)
		{
			this.WriteLine(this.Prefix(Timestamp, "Rx") + Text);
		}

		/// <summary>
		/// Writes a line of text.
		/// </summary>
		/// <param name="s">String to write.</param>
		protected abstract void WriteLine(string s);

		/// <summary>
		/// <see cref="ISniffer.TransmitText(DateTime, String)"/>
		/// </summary>
		public override void TransmitText(DateTime Timestamp, string Text)
		{
			this.WriteLine(this.Prefix(Timestamp, "Tx") + Text);
		}

		/// <summary>
		/// <see cref="ISniffer.Information(DateTime, String)"/>
		/// </summary>
		public override void Information(DateTime Timestamp, string Comment)
		{
			this.WriteLine(this.Prefix(Timestamp, "Info") + Comment);
		}

		/// <summary>
		/// <see cref="ISniffer.Warning(DateTime, String)"/>
		/// </summary>
		public override void Warning(DateTime Timestamp, string Warning)
		{
			this.WriteLine(this.Prefix(Timestamp, "Warning") + Warning);
		}

		/// <summary>
		/// <see cref="ISniffer.Error(DateTime, String)"/>
		/// </summary>
		public override void Error(DateTime Timestamp, string Error)
		{
			this.WriteLine(this.Prefix(Timestamp, "Error") + Error);
		}

		/// <summary>
		/// <see cref="ISniffer.Exception(DateTime, String)"/>
		/// </summary>
		public override void Exception(DateTime Timestamp, string Exception)
		{
			this.WriteLine(this.Prefix(Timestamp, "Exception") + Exception);
		}
	}
}
