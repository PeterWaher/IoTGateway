using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to <see cref="Console.Out"/>.
	/// </summary>
	public class ConsoleOutSniffer : ISniffer
	{
		private const int TabWidth = 8;

		/// <summary>
		/// Outputs sniffed data to <see cref="Console.Out"/>.
		/// </summary>
		public ConsoleOutSniffer()
		{
		}

		public void TransmitText(string Text)
		{
			this.Output(Text, ConsoleColor.Black, ConsoleColor.White);
		}

		public void ReceiveText(string Text)
		{
			this.Output(Text, ConsoleColor.White, ConsoleColor.DarkBlue);
		}

		public void TransmitBinary(byte[] Data)
		{
			this.HexOutput(Data, ConsoleColor.Black, ConsoleColor.White);
		}

		public void ReceiveBinary(byte[] Data)
		{
			this.HexOutput(Data, ConsoleColor.White, ConsoleColor.DarkBlue);
		}

		private void HexOutput(byte[] Data, ConsoleColor Fg, ConsoleColor Bg)
		{
			StringBuilder Row = new StringBuilder();
			int i = 0;

			foreach (byte b in Data)
			{
				if (i > 0)
					Row.Append(' ');

				Row.Append(b.ToString("X2"));

				i = (i + 1) & 31;
				if (i == 0)
				{
					this.Output(Row.ToString(), Fg, Bg);
					Row.Clear();
				}
			}

			if (i != 0)
				this.Output(Row.ToString(), Fg, Bg);
		}

		public void Information(string Comment)
		{
			this.Output(Comment, ConsoleColor.Yellow, ConsoleColor.DarkGreen);
		}

		public void Warning(string Warning)
		{
			this.Output(Warning, ConsoleColor.Black, ConsoleColor.Yellow);
		}

		public void Error(string Error)
		{
			this.Output(Error, ConsoleColor.Yellow, ConsoleColor.Red);
		}

		public void Exception(string Exception)
		{
			this.Output(Exception, ConsoleColor.White, ConsoleColor.DarkRed);
		}

		private void Output(string s, ConsoleColor Fg, ConsoleColor Bg)
		{
			ConsoleColor FgBak = Console.ForegroundColor;
			ConsoleColor BgBak = Console.BackgroundColor;

			Console.ForegroundColor = Fg;
			Console.BackgroundColor = Bg;

			try
			{
				int w = Console.WindowWidth;
				int i;

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
							i = Console.CursorLeft % TabWidth;
							sb.Append(new string(' ', TabWidth - i));
						}

						sb.Append(Part);
					}

					s = sb.ToString();
				}

				i = s.Length % w;

				if (i > 0)
					s += new string(' ', w - i);
	
				Console.Out.Write(s);
			}
			catch (Exception)
			{
				Console.Out.WriteLine(s);
			}

			Console.ForegroundColor = Fg;
			Console.BackgroundColor = Bg;
		}

	}
}
