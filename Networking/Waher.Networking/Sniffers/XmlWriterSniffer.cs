using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to an XML writer.
	/// </summary>
	public class XmlWriterSniffer : SnifferBase, IDisposable
	{
		/// <summary>
		/// XML output writer.
		/// </summary>
		protected XmlWriter output;

		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
		private readonly BinaryPresentationMethod binaryPresentationMethod;
		private bool disposed = false;

		/// <summary>
		/// Outputs sniffed data to an XML writer.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public XmlWriterSniffer(XmlWriter Output, BinaryPresentationMethod BinaryPresentationMethod)
		{
			this.output = Output;
			this.binaryPresentationMethod = BinaryPresentationMethod;
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
				return this.HexOutput(Timestamp, Data, "Rx");
			else
				return Task.CompletedTask;
		}

		private async Task HexOutput(DateTime Timestamp, byte[] Data, string TagName)
		{
			if (this.disposed)
				return;

			await this.semaphore.WaitAsync();
			try
			{
				await this.BeforeWrite();
				if (!(this.output is null))
				{
					try
					{
						this.output.WriteStartElement(TagName);
						this.output.WriteAttributeString("timestamp", Encode(Timestamp));

						switch (this.binaryPresentationMethod)
						{
							case BinaryPresentationMethod.Hexadecimal:
								StringBuilder sb = new StringBuilder();
								int i = 0;

								foreach (byte b in Data)
								{
									if (i > 0)
										sb.Append(' ');

									sb.Append(b.ToString("X2"));

									i = (i + 1) & 31;
									if (i == 0)
									{
										this.output.WriteElementString("Row", sb.ToString());
										sb.Clear();
									}
								}

								if (i != 0)
									this.output.WriteElementString("Row", sb.ToString());
								break;

							case BinaryPresentationMethod.Base64:

								string s = Convert.ToBase64String(Data);

								while (!string.IsNullOrEmpty(s))
								{
									if (s.Length > 76)
									{
										this.output.WriteElementString("Row", s.Substring(0, 76));
										s = s.Substring(76);
									}
									else
									{
										this.output.WriteElementString("Row", s);
										s = null;
									}
								}
								break;

							case BinaryPresentationMethod.ByteCount:
								this.output.WriteElementString("Row", "<" + Data.Length.ToString() + " bytes>");
								break;
						}

						this.output.WriteEndElement();
						this.output.Flush();
					}
					finally
					{
						await this.AfterWrite();
					}
				}
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		internal static string Encode(DateTime DT)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(DT.Year.ToString("D4"));
			sb.Append('-');
			sb.Append(DT.Month.ToString("D2"));
			sb.Append('-');
			sb.Append(DT.Day.ToString("D2"));
			sb.Append('T');
			sb.Append(DT.Hour.ToString("D2"));
			sb.Append(':');
			sb.Append(DT.Minute.ToString("D2"));
			sb.Append(':');
			sb.Append(DT.Second.ToString("D2"));
			sb.Append('.');
			sb.Append(DT.Millisecond.ToString("D3"));

			if (DT.Kind == DateTimeKind.Utc)
				sb.Append("Z");

			return sb.ToString();
		}

		/// <inheritdoc/>
		public override Task TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			if (Data.Length > 0)
				return this.HexOutput(Timestamp, Data, "Tx");
			else
				return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public override Task ReceiveText(DateTime Timestamp, string Text)
		{
			return this.WriteLine(Timestamp, "Rx", Text);
		}

		private async Task WriteLine(DateTime Timestamp, string TagName, string Text)
		{
			if (this.disposed)
				return;

			int i = Text.IndexOfAny(controlCharacters);
			int j;
			string s;
			char ch;

			while (i >= 0)
			{
				ch = Text[i];
				j = Array.IndexOf(controlCharacters, ch);
				s = controlCharacterNames[j];
				Text = Text.Remove(i, 1).Insert(i, " " + s + " ");
				i += s.Length + 2;
				i = Text.IndexOfAny(controlCharacters, i);
			}

			await this.semaphore.WaitAsync();
			try
			{
				try
				{
					await this.BeforeWrite();
					if (!(this.output is null))
					{
						try
						{
							this.output.WriteStartElement(TagName);
							this.output.WriteAttributeString("timestamp", Encode(Timestamp));

							foreach (string Row in GetRows(Text))
								this.output.WriteElementString("Row", Row);

							this.output.WriteEndElement();
							this.output.Flush();
						}
						finally
						{
							await this.AfterWrite();
						}
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
			finally
			{
				this.semaphore.Release();
			}
		}

		private static readonly char[] controlCharacters = new char[]
		{
			(char)00, (char)01, (char)02, (char)03, (char)04, (char)05, (char)06, (char)07, (char)08,
			(char)11, (char)12, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19,
			(char)20, (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29,
			(char)30, (char)31
		};

		private static readonly string[] controlCharacterNames = new string[]
		{
			"NUL", "SOH", "STX", "ETX", "EOT", "ENQ", "ACK", "BEL", "BS",
			"VT", "FF", "SO", "SI", "DLE", "DC1", "DC2", "DC3",
			"DC4", "NAK", "SYN", "ETB", "CAN", "EM", "SUB", "ESC", "FS", "GS",
			"RS", "US"
		};

		private static string[] GetRows(string s)
		{
			return s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		}

		/// <inheritdoc/>
		public override Task TransmitText(DateTime Timestamp, string Text)
		{
			return this.WriteLine(Timestamp, "Tx", Text);
		}

		/// <inheritdoc/>
		public override Task Information(DateTime Timestamp, string Comment)
		{
			return this.WriteLine(Timestamp, "Info", Comment);
		}

		/// <inheritdoc/>
		public override Task Warning(DateTime Timestamp, string Warning)
		{
			return this.WriteLine(Timestamp, "Warning", Warning);
		}

		/// <inheritdoc/>
		public override Task Error(DateTime Timestamp, string Error)
		{
			return this.WriteLine(Timestamp, "Error", Error);
		}

		/// <inheritdoc/>
		public override Task Exception(DateTime Timestamp, string Exception)
		{
			return this.WriteLine(Timestamp, "Exception", Exception);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			this.disposed = true;
			this.semaphore.Dispose();
		}
	}
}
