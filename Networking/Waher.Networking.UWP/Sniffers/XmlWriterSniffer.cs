using System;
using System.Text;
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

		private readonly BinaryPresentationMethod binaryPresentationMethod;
		private readonly object synchObject = new object();
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
				this.HexOutput(Timestamp, Data, "Rx");
		}

		private void HexOutput(DateTime Timestamp, byte[] Data, string TagName)
		{
			if (this.disposed)
				return;

			lock (this.synchObject)
			{
				this.BeforeWrite();
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
						this.AfterWrite();
					}
				}
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

		/// <summary>
		/// <see cref="ISniffer.TransmitBinary(DateTime, byte[])"/>
		/// </summary>
		public override void TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			if (Data.Length > 0)
				this.HexOutput(Timestamp, Data, "Tx");
		}

		/// <summary>
		/// <see cref="ISniffer.ReceiveText(DateTime, String)"/>
		/// </summary>
		public override void ReceiveText(DateTime Timestamp, string Text)
		{
			this.WriteLine(Timestamp, "Rx", Text);
		}

		private void WriteLine(DateTime Timestamp, string TagName, string Text)
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
				j = Array.IndexOf<char>(controlCharacters, ch);
				s = controlCharacterNames[j];
				Text = Text.Remove(i, 1).Insert(i, " " + s + " ");
				i += s.Length + 2;
				i = Text.IndexOfAny(controlCharacters, i);
			}

			lock (this.synchObject)
			{
				try
				{
					this.BeforeWrite();
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
							this.AfterWrite();
						}
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
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

		/// <summary>
		/// <see cref="ISniffer.TransmitText(DateTime, String)"/>
		/// </summary>
		public override void TransmitText(DateTime Timestamp, string Text)
		{
			this.WriteLine(Timestamp, "Tx", Text);
		}

		/// <summary>
		/// <see cref="ISniffer.Information(DateTime, String)"/>
		/// </summary>
		public override void Information(DateTime Timestamp, string Comment)
		{
			this.WriteLine(Timestamp, "Info", Comment);
		}

		/// <summary>
		/// <see cref="ISniffer.Warning(DateTime, String)"/>
		/// </summary>
		public override void Warning(DateTime Timestamp, string Warning)
		{
			this.WriteLine(Timestamp, "Warning", Warning);
		}

		/// <summary>
		/// <see cref="ISniffer.Error(DateTime, String)"/>
		/// </summary>
		public override void Error(DateTime Timestamp, string Error)
		{
			this.WriteLine(Timestamp, "Error", Error);
		}

		/// <summary>
		/// <see cref="ISniffer.Exception(DateTime, String)"/>
		/// </summary>
		public override void Exception(DateTime Timestamp, string Exception)
		{
			this.WriteLine(Timestamp, "Exception", Exception);
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
