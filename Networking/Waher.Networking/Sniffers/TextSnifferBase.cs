using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers.Model;

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
			get => this.includeTimestamp;
			set => this.includeTimestamp = value;
		}

		/// <summary>
		/// How the sniffer handles binary data.
		/// </summary>
		public override BinaryPresentationMethod BinaryPresentationMethod => this.binaryPresentationMethod;

		/// <summary>
		/// Method is called before writing something to the text file.
		/// </summary>
		protected virtual Task BeforeWrite()
		{
			this.OnBeforeWrite.Raise(this, EventArgs.Empty);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Method is called after writing something to the text file.
		/// </summary>
		protected virtual Task AfterWrite()
		{
			this.OnAfterWrite.Raise(this, EventArgs.Empty);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Event raised before a write operation takes place.
		/// </summary>
		public event EventHandler OnBeforeWrite;

		/// <summary>
		/// Event raised before a write operation takes place.
		/// </summary>
		public event EventHandler OnAfterWrite;

		/// <summary>
		/// Processes a binary reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferRxBinary Event, CancellationToken Cancel)
		{
			return this.HexOutput(Event.Data, Event.Offset, Event.Count, this.Prefix(Event.Timestamp, "Rx"));
		}

		private string Prefix(DateTime Timestamp, string Type)
		{
			if (this.includeTimestamp)
				return Type + " (" + Timestamp.ToString("T") + "): ";
			else
				return Type + ": ";
		}

		private async Task HexOutput(byte[] Data, int Offset, int Count, string RowPrefix)
		{
			await this.BeforeWrite();
			try
			{
				switch (Data is null ? BinaryPresentationMethod.ByteCount : this.binaryPresentationMethod)
				{
					case BinaryPresentationMethod.Hexadecimal:
						StringBuilder sb = new StringBuilder();
						int i = 0;
						byte b;

						while (Count-- > 0)
						{
							b = Data[Offset++];

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
						await this.WriteLine(RowPrefix + Convert.ToBase64String(Data, Offset, Count));
						break;

					case BinaryPresentationMethod.ByteCount:
						await this.WriteLine(RowPrefix + "<" + Count.ToString() + " bytes>");
						break;
				}
			}
			finally
			{
				await this.AfterWrite();
			}
		}

		/// <summary>
		/// Processes a binary transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferTxBinary Event, CancellationToken Cancel)
		{
			return this.HexOutput(Event.Data, Event.Offset, Event.Count, this.Prefix(Event.Timestamp, "Tx"));
		}

		/// <summary>
		/// Processes a text reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferRxText Event, CancellationToken Cancel)
		{
			return this.WriteLine(this.Prefix(Event.Timestamp, "Rx") + Event.Text);
		}

		/// <summary>
		/// Writes a line of text.
		/// </summary>
		/// <param name="s">String to write.</param>
		protected abstract Task WriteLine(string s);

		/// <summary>
		/// Processes a text transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferTxText Event, CancellationToken Cancel)
		{
			return this.WriteLine(this.Prefix(Event.Timestamp, "Tx") + Event.Text);
		}

		/// <summary>
		/// Processes an information event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferInformation Event, CancellationToken Cancel)
		{
			return this.WriteLine(this.Prefix(Event.Timestamp, "Info") + Event.Text);
		}

		/// <summary>
		/// Processes a warning event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferWarning Event, CancellationToken Cancel)
		{
			return this.WriteLine(this.Prefix(Event.Timestamp, "Warning") + Event.Text);
		}

		/// <summary>
		/// Processes an error event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferError Event, CancellationToken Cancel)
		{
			return this.WriteLine(this.Prefix(Event.Timestamp, "Error") + Event.Text);
		}

		/// <summary>
		/// Processes an exception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferException Event, CancellationToken Cancel)
		{
			return this.WriteLine(this.Prefix(Event.Timestamp, "Exception") + Event.Text);
		}
	}
}
