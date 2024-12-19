using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Console;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// How binary data is to be presented.
	/// </summary>
	public enum BinaryPresentationMethod
	{
		/// <summary>
		/// Has hexadecimal strings.
		/// </summary>
		Hexadecimal,

		/// <summary>
		/// Has base64 strings.
		/// </summary>
		Base64,

		/// <summary>
		/// Has simple byte counts.
		/// </summary>
		ByteCount
	}

	/// <summary>
	/// Type of line ending.
	/// </summary>
	public enum LineEnding
	{
		/// <summary>
		/// Pad with spaces until next rows. Makes sure line is colored properly.
		/// </summary>
		PadWithSpaces,

		/// <summary>
		/// End with new line characters. Is easier to read in a text editor.
		/// </summary>
		NewLine
	}

	/// <summary>
	/// Outputs sniffed data to the Console Output, serialized by <see cref="ConsoleOut"/>.
	/// </summary>
	public class ConsoleOutSniffer : SnifferBase, IDisposable
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
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		public override Task TransmitText(DateTime Timestamp, string Text)
		{
			this.Output(Timestamp, Text, ConsoleColor.Black, ConsoleColor.White);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		public override Task ReceiveText(DateTime Timestamp, string Text)
		{
			this.Output(Timestamp, Text, ConsoleColor.White, ConsoleColor.DarkBlue);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		public override Task TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			return this.BinaryOutput(Timestamp, Data, 0, Data.Length, ConsoleColor.Black, ConsoleColor.White);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public override Task TransmitBinary(DateTime Timestamp, byte[] Data, int Offset, int Count)
		{
			return this.BinaryOutput(Timestamp, Data, Offset, Count, ConsoleColor.Black, ConsoleColor.White);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		public override Task ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			return this.BinaryOutput(Timestamp, Data, 0, Data.Length, ConsoleColor.White, ConsoleColor.DarkBlue);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public override Task ReceiveBinary(DateTime Timestamp, byte[] Data, int Offset, int Count)
		{
			return this.BinaryOutput(Timestamp, Data, Offset, Count, ConsoleColor.White, ConsoleColor.DarkBlue);
		}

		private Task BinaryOutput(DateTime Timestamp, byte[] Data, int Offset, int Count, ConsoleColor Fg, ConsoleColor Bg)
		{
			switch (this.binaryPresentationMethod)
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

			return Task.CompletedTask;
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		public override Task Information(DateTime Timestamp, string Comment)
		{
			this.Output(Timestamp, Comment, ConsoleColor.Yellow, ConsoleColor.DarkGreen);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		public override Task Warning(DateTime Timestamp, string Warning)
		{
			this.Output(Timestamp, Warning, ConsoleColor.Black, ConsoleColor.Yellow);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		public override Task Error(DateTime Timestamp, string Error)
		{
			this.Output(Timestamp, Error, ConsoleColor.Yellow, ConsoleColor.Red);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		public override Task Exception(DateTime Timestamp, string Exception)
		{
			this.Output(Timestamp, Exception, ConsoleColor.White, ConsoleColor.DarkRed);
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

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
		}
	}
}
