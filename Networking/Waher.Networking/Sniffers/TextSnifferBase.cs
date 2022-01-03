using System;
using System.Text;
using System.Threading.Tasks;

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
		protected virtual Task BeforeWrite()
		{
			return Task.CompletedTask;  // Do nothing by default.
		}

		/// <summary>
		/// Method is called after writing something to the text file.
		/// </summary>
		protected virtual Task AfterWrite()
		{
			return Task.CompletedTask;  // Do nothing by default.
		}

		/// <inheritdoc/>
		public override Task ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			if (Data.Length > 0)
				return this.HexOutput(Data, this.Prefix(Timestamp, "Rx"));
			else
				return Task.CompletedTask;
		}

		private string Prefix(DateTime Timestamp, string Type)
		{
			if (this.includeTimestamp)
				return Type + " (" + Timestamp.ToString("T") + "): ";
			else
				return Type + ": ";
		}

		private async Task HexOutput(byte[] Data, string RowPrefix)
		{
			await this.BeforeWrite();
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
								await this.WriteLine(sb.ToString());
								sb.Clear();
							}
						}

						if (i != 0)
							await this.WriteLine(sb.ToString());
						break;

					case BinaryPresentationMethod.Base64:
						await this.WriteLine(RowPrefix + Convert.ToBase64String(Data));
						break;

					case BinaryPresentationMethod.ByteCount:
						await this.WriteLine(RowPrefix + "<" + Data.Length.ToString() + " bytes>");
						break;
				}
			}
			finally
			{
				await this.AfterWrite();
			}
		}

		/// <inheritdoc/>
		public override Task TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			if (Data.Length > 0)
				return this.HexOutput(Data, this.Prefix(Timestamp, "Tx"));
			else
				return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public override Task ReceiveText(DateTime Timestamp, string Text)
		{
			return this.WriteLine(this.Prefix(Timestamp, "Rx") + Text);
		}

		/// <summary>
		/// Writes a line of text.
		/// </summary>
		/// <param name="s">String to write.</param>
		protected abstract Task WriteLine(string s);

		/// <inheritdoc/>
		public override Task TransmitText(DateTime Timestamp, string Text)
		{
			return this.WriteLine(this.Prefix(Timestamp, "Tx") + Text);
		}

		/// <inheritdoc/>
		public override Task Information(DateTime Timestamp, string Comment)
		{
			return this.WriteLine(this.Prefix(Timestamp, "Info") + Comment);
		}

		/// <inheritdoc/>
		public override Task Warning(DateTime Timestamp, string Warning)
		{
			return this.WriteLine(this.Prefix(Timestamp, "Warning") + Warning);
		}

		/// <inheritdoc/>
		public override Task Error(DateTime Timestamp, string Error)
		{
			return this.WriteLine(this.Prefix(Timestamp, "Error") + Error);
		}

		/// <inheritdoc/>
		public override Task Exception(DateTime Timestamp, string Exception)
		{
			return this.WriteLine(this.Prefix(Timestamp, "Exception") + Exception);
		}
	}
}
