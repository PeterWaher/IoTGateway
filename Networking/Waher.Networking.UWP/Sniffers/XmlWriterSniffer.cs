using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to an XML writer.
	/// </summary>
	public class XmlWriterSniffer : ISniffer, IDisposable
	{
		protected XmlWriter output;
		private BinaryPresentationMethod binaryPresentationMethod;
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
		/// <see cref="ISniffer.ReceiveBinary"/>
		/// </summary>
		public virtual void ReceiveBinary(byte[] Data)
		{
			if (Data.Length > 0)
				this.HexOutput(Data, "Rx");
		}

		private void HexOutput(byte[] Data, string TagName)
		{
			if (this.disposed)
				return;

			this.BeforeWrite();
			try
			{
				this.output.WriteStartElement(TagName);
				this.output.WriteAttributeString("timestamp", Encode(DateTime.Now));

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
#if WINDOWS_UWP
						this.output.WriteElementString("Row", System.Convert.ToBase64String(Data));
#else
						foreach (string Row in System.Convert.ToBase64String(Data,
							Base64FormattingOptions.InsertLineBreaks).Split(ConsoleOutSniffer.CRLF))
						{
							this.output.WriteElementString("Row", Row);
						}
#endif
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
		/// <see cref="ISniffer.TransmitBinary"/>
		/// </summary>
		public virtual void TransmitBinary(byte[] Data)
		{
			if (Data.Length > 0)
				this.HexOutput(Data, "Tx");
		}

		/// <summary>
		/// <see cref="ISniffer.ReceiveText"/>
		/// </summary>
		public virtual void ReceiveText(string Text)
		{
			this.WriteLine("Rx", Text);
		}

		private void WriteLine(string TagName, string Text)
		{
			if (this.disposed)
				return;

			this.BeforeWrite();
			try
			{
				this.output.WriteStartElement(TagName);
				this.output.WriteAttributeString("timestamp", Encode(DateTime.Now));

#if WINDOWS_UWP
				this.output.WriteElementString("Row", Text);
#else
				foreach (string Row in Text.Split(ConsoleOutSniffer.CRLF))
					this.output.WriteElementString("Row", Row);
#endif
				this.output.WriteEndElement();
				this.output.Flush();
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
			this.WriteLine("Tx", Text);
		}

		/// <summary>
		/// <see cref="ISniffer.Information"/>
		/// </summary>
		public virtual void Information(string Comment)
		{
			this.WriteLine("Info", Comment);
		}

		/// <summary>
		/// <see cref="ISniffer.Warning"/>
		/// </summary>
		public virtual void Warning(string Warning)
		{
			this.WriteLine("Warning", Warning);
		}

		/// <summary>
		/// <see cref="ISniffer.Error"/>
		/// </summary>
		public virtual void Error(string Error)
		{
			this.WriteLine("Error", Error);
		}

		/// <summary>
		/// <see cref="ISniffer.Exception"/>
		/// </summary>
		public virtual void Exception(string Exception)
		{
			this.WriteLine("Exception", Exception);
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
