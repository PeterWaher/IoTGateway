using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Networking.Sniffers.Model;
using Waher.Runtime.Threading;

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

		private readonly MultiReadSingleWriteObject semaphore;
		private readonly BinaryPresentationMethod binaryPresentationMethod;
		private DateTime lastEvent = DateTime.MinValue;
		private bool disposed = false;

		/// <summary>
		/// Outputs sniffed data to an XML writer.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public XmlWriterSniffer(XmlWriter Output, BinaryPresentationMethod BinaryPresentationMethod)
		{
			this.semaphore = new MultiReadSingleWriteObject(this);
			this.output = Output;
			this.binaryPresentationMethod = BinaryPresentationMethod;
		}

		/// <summary>
		/// Timestamp of Last event
		/// </summary>
		public DateTime LastEvent => this.lastEvent;

		/// <summary>
		/// How the sniffer handles binary data.
		/// </summary>
		public override BinaryPresentationMethod BinaryPresentationMethod => this.binaryPresentationMethod;

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

		/// <summary>
		/// Processes a binary reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferRxBinary Event)
		{
			this.lastEvent = Event.Timestamp;

			return this.HexOutput(Event.Timestamp, Event.Data, Event.Offset, Event.Count, "Rx");
		}

		private async Task HexOutput(DateTime Timestamp, byte[] Data, int Offset, int Count, string TagName)
		{
			if (this.disposed)
				return;

			await this.semaphore.BeginWrite();
			try
			{
				await this.BeforeWrite();
				if (!(this.output is null))
				{
					try
					{
						this.output.WriteStartElement(TagName);
						this.output.WriteAttributeString("timestamp", Encode(Timestamp));

						switch (Data is null ? BinaryPresentationMethod.ByteCount : this.binaryPresentationMethod)
						{
							case BinaryPresentationMethod.Hexadecimal:
								StringBuilder sb = new StringBuilder();
								int i = 0;
								byte b;

								while (Count-- > 0)
								{
									b = Data[Offset++];

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

								string s = Convert.ToBase64String(Data, Offset, Count);

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
								this.output.WriteElementString("Row", "<" + Count.ToString() + " bytes>");
								break;
						}

						this.output.WriteEndElement();
						this.output.Flush();
					}
					catch (Exception)
					{
						try
						{
							this.DisposeOutput();
						}
						catch (Exception)
						{
							// Ignore
						}
					}
					finally
					{
						await this.AfterWrite();
					}
				}
			}
			finally
			{
				await this.semaphore.EndWrite();
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
		/// Processes a binary transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferTxBinary Event)
		{
			return this.HexOutput(Event.Timestamp, Event.Data, Event.Offset, Event.Count, "Tx");
		}

		/// <summary>
		/// Processes a text reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferRxText Event)
		{
			return this.WriteLine(Event.Timestamp, "Rx", Event.Text);
		}

		private async Task WriteLine(DateTime Timestamp, string TagName, string Text)
		{
			if (this.disposed)
				return;

			Text = CommunicationLayer.EncodeControlCharacters(Text);

			await this.semaphore.BeginWrite();
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
				catch (Exception)
				{
					try
					{
						this.DisposeOutput();
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
			finally
			{
				if (!this.disposed)
					await this.semaphore.EndWrite();
			}
		}

		private static string[] GetRows(string s)
		{
			return s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		}

		/// <summary>
		/// Processes a text transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferTxText Event)
		{
			return this.WriteLine(Event.Timestamp, "Tx", Event.Text);
		}

		/// <summary>
		/// Processes an information event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferInformation Event)
		{
			return this.WriteLine(Event.Timestamp, "Info", Event.Text);
		}

		/// <summary>
		/// Processes a warning event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferWarning Event)
		{
			return this.WriteLine(Event.Timestamp, "Warning", Event.Text);
		}

		/// <summary>
		/// Processes an error event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferError Event)
		{
			return this.WriteLine(Event.Timestamp, "Error", Event.Text);
		}

		/// <summary>
		/// Processes an exception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferException Event)
		{
			return this.WriteLine(Event.Timestamp, "Exception", Event.Text);
		}

		/// <summary>
		/// Disposes of the current output.
		/// </summary>
		public virtual void DisposeOutput()
		{
			this.output?.Flush();
			this.output?.Dispose();
			this.output = null;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override async Task DisposeAsync()
		{
			if (!this.disposed)
			{
				await this.semaphore.BeginWrite();
				try
				{
					if (!this.disposed)
					{
						this.disposed = true;

						await base.DisposeAsync();

						this.DisposeOutput();
						this.semaphore.Dispose();   // End writing
					}
				}
				catch (Exception ex)
				{
					await this.semaphore.EndWrite();    // Only needed in case there's an Exception
					Log.Exception(ex);
				}
			}
		}
	}
}
