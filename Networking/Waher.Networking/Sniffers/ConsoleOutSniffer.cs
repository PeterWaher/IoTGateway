using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.Sniffers.Model;
using Waher.Runtime.Console;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to the Console Output, serialized by <see cref="ConsoleOut"/>.
	/// </summary>
	public class ConsoleOutSniffer : SnifferBase
	{
		private const int TabWidth = 8;
		private readonly BinaryPresentationMethod binaryPresentationMethod;
		private readonly LineEnding lineEndingMethod;
		private bool consoleWidthWorks = true;
		private bool includeTimestamp = false;

		/// <summary>
		/// Outputs sniffed data to the Console Output, serialized by <see cref="ConsoleOut"/>.
		/// </summary>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		/// <param name="LineEndingMethod">Line ending method.</param>
		public ConsoleOutSniffer(BinaryPresentationMethod BinaryPresentationMethod,
			LineEnding LineEndingMethod)
			: base("Console Out Sniffer")
		{
			this.binaryPresentationMethod = BinaryPresentationMethod;
			this.lineEndingMethod = LineEndingMethod;
		}

		/// <summary>
		/// If timestamps should be included in the output. (Default is false.)
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
		/// Processes a text transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferTxText Event, CancellationToken Cancel)
		{
			this.Output(Event.Timestamp, Event.Text, ConsoleColor.Black, ConsoleColor.White);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes a text reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferRxText Event, CancellationToken Cancel)
		{
			this.Output(Event.Timestamp, Event.Text, ConsoleColor.White, ConsoleColor.DarkBlue);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes a binary transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferTxBinary Event, CancellationToken Cancel)
		{
			this.BinaryOutput(Event.Timestamp, Event.Data, Event.Offset, Event.Count, ConsoleColor.Black, ConsoleColor.White);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes a binary reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferRxBinary Event, CancellationToken Cancel)
		{
			this.BinaryOutput(Event.Timestamp, Event.Data, Event.Offset, Event.Count, ConsoleColor.White, ConsoleColor.DarkBlue);
			return Task.CompletedTask;
		}

		private void BinaryOutput(DateTime Timestamp, byte[] Data, int Offset, int Count, ConsoleColor Fg, ConsoleColor Bg)
		{
			switch (Data is null ? BinaryPresentationMethod.ByteCount : this.binaryPresentationMethod)
			{
				case BinaryPresentationMethod.Hexadecimal:
					StringBuilder Row = new StringBuilder();
					int i = 0;
					byte b;

					while (Count-- > 0)
					{
						b = Data[Offset++];

						if (i > 0)
							Row.Append(' ');

						Row.Append(b.ToString("X2"));

						i = (i + 1) & 31;
						if (i == 0)
						{
							this.Output(Timestamp, Row.ToString(), Fg, Bg);
							Row.Clear();
						}
					}

					if (i != 0)
						this.Output(Timestamp, Row.ToString(), Fg, Bg);
					break;

				case BinaryPresentationMethod.Base64:
					this.Output(Timestamp, Convert.ToBase64String(Data, Offset, Count), Fg, Bg);
					break;

				case BinaryPresentationMethod.ByteCount:
					this.Output(Timestamp, "<" + Count.ToString() + " bytes>", Fg, Bg);
					break;
			}
		}

		/// <summary>
		/// Processes an information event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferInformation Event, CancellationToken Cancel)
		{
			this.Output(Event.Timestamp, Event.Text, ConsoleColor.Yellow, ConsoleColor.DarkGreen);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes a warning event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferWarning Event, CancellationToken Cancel)
		{
			this.Output(Event.Timestamp, Event.Text, ConsoleColor.Black, ConsoleColor.Yellow);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes an error event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferError Event, CancellationToken Cancel)
		{
			this.Output(Event.Timestamp, Event.Text, ConsoleColor.Yellow, ConsoleColor.Red);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes an exception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferException Event, CancellationToken Cancel)
		{
			this.Output(Event.Timestamp, Event.Text, ConsoleColor.White, ConsoleColor.DarkRed);
			return Task.CompletedTask;
		}

		private void Output(DateTime Timestamp, string s, ConsoleColor Fg, ConsoleColor Bg)
		{
			ConsoleOut.Write((Output) =>
			{
				if (this.includeTimestamp)
					s = Timestamp.ToString("T") + ": " + s;

				if (this.lineEndingMethod == LineEnding.PadWithSpaces)
				{
					int i, w;

					if (this.consoleWidthWorks)
					{
						try
						{
							w = ConsoleOut.WindowWidth;

							foreach (string Row in s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
							{
								s = Row;

								if (s.IndexOf('\t') >= 0)
								{
									StringBuilder sb = new StringBuilder();
									string[] Parts = s.Split('\t');
									bool First = true;

									foreach (string Part in Parts)
									{
										if (First)
											First = false;
										else
										{
											i = ConsoleOut.CursorLeft % TabWidth;
											sb.Append(new string(' ', TabWidth - i));
										}

										sb.Append(Part);
									}

									s = sb.ToString();
								}

								i = s.Length % w;

								if (i > 0)
									s += new string(' ', w - i);

								Output.Write(s);
							}
						}
						catch (Exception)
						{
							this.consoleWidthWorks = false;
							Output.WriteLine(s);
						}
					}
					else
						Output.WriteLine(s);
				}
				else
					Output.WriteLine(s);
			}, Fg, Bg);
		}

		internal static readonly char[] CRLF = new char[] { '\r', '\n' };
	}
}
