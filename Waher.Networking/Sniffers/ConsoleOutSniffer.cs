using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to <see cref="Console.Out"/>.
	/// </summary>
	public class ConsoleOutSniffer : TextWriterSniffer
	{
		/// <summary>
		/// Outputs sniffed data to <see cref="Console.Out"/>.
		/// </summary>
		public ConsoleOutSniffer()
			: base(Console.Out)
		{
		}

		public override void TransmitText(string Text)
		{
			ConsoleColor Fg = Console.ForegroundColor;
			ConsoleColor Bg = Console.BackgroundColor;

			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.White;

			base.TransmitText(Text);

			Console.ForegroundColor = Fg;
			Console.BackgroundColor = Bg;
		}

		public override void ReceiveText(string Text)
		{
			ConsoleColor Fg = Console.ForegroundColor;
			ConsoleColor Bg = Console.BackgroundColor;

			Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.DarkBlue;

			base.ReceiveText(Text);

			Console.ForegroundColor = Fg;
			Console.BackgroundColor = Bg;
		}

		public override void TransmitBinary(byte[] Data)
		{
			ConsoleColor Fg = Console.ForegroundColor;
			ConsoleColor Bg = Console.BackgroundColor;

			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.White;

			base.TransmitBinary(Data);

			Console.ForegroundColor = Fg;
			Console.BackgroundColor = Bg;
		}

		public override void ReceiveBinary(byte[] Data)
		{
			ConsoleColor Fg = Console.ForegroundColor;
			ConsoleColor Bg = Console.BackgroundColor;

			Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.DarkBlue;

			base.ReceiveBinary(Data);

			Console.ForegroundColor = Fg;
			Console.BackgroundColor = Bg;
		}

		public override void Information(string Comment)
		{
			ConsoleColor Fg = Console.ForegroundColor;
			ConsoleColor Bg = Console.BackgroundColor;

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.BackgroundColor = ConsoleColor.DarkGreen;

			base.Information(Comment);

			Console.ForegroundColor = Fg;
			Console.BackgroundColor = Bg;
		}

		public override void Warning(string Warning)
		{
			ConsoleColor Fg = Console.ForegroundColor;
			ConsoleColor Bg = Console.BackgroundColor;

			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.Yellow;

			base.Warning(Warning);

			Console.ForegroundColor = Fg;
			Console.BackgroundColor = Bg;
		}

		public override void Error(string Error)
		{
			ConsoleColor Fg = Console.ForegroundColor;
			ConsoleColor Bg = Console.BackgroundColor;

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.BackgroundColor = ConsoleColor.Red;

			base.Error(Error);

			Console.ForegroundColor = Fg;
			Console.BackgroundColor = Bg;
		}

		public override void Exception(string Exception)
		{
			ConsoleColor Fg = Console.ForegroundColor;
			ConsoleColor Bg = Console.BackgroundColor;

			Console.ForegroundColor = ConsoleColor.White;
			Console.BackgroundColor = ConsoleColor.DarkRed;

			base.Exception(Exception);

			Console.ForegroundColor = Fg;
			Console.BackgroundColor = Bg;
		}

		// TODO: Instead of WriteLine() append spaces to color code rest of line.
	}
}
