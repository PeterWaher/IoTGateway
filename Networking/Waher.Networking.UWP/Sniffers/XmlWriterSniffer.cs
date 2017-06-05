using System;
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
		private object synchObject = new object();

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

			lock (this.synchObject)
			{
				this.BeforeWrite();
				if (this.output != null)
				{
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

								string s = System.Convert.ToBase64String(Data);

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

			lock (this.synchObject)
			{
				this.BeforeWrite();
				if (this.output != null)
				{
					try
					{
						this.output.WriteStartElement(TagName);
						this.output.WriteAttributeString("timestamp", Encode(DateTime.Now));

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
		}

		private static string[] GetRows(string s)
		{
			return s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
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
