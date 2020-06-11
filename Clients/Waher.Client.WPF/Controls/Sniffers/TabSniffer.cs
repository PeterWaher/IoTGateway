using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Waher.Networking.Sniffers;
using Waher.Client.WPF.Controls;

namespace Waher.Client.WPF.Controls.Sniffers
{
	public class TabSniffer : SnifferBase
	{
		private readonly SnifferView view;
		private string snifferId = null;

		public TabSniffer(SnifferView View)
		{
			this.view = View;
		}

		public string SnifferId
		{
			get { return this.snifferId; }
			set { this.snifferId = value; }
		}

		public override void ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			this.view.Add(new SniffItem(Timestamp, SniffItemType.DataReceived, HexToString(Data), Data, Colors.White, Colors.Navy));
		}

		public override void TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			this.view.Add(new SniffItem(Timestamp, SniffItemType.DataTransmitted, HexToString(Data), Data, Colors.Black, Colors.White));
		}

		internal static string HexToString(byte[] Data)
		{
			StringBuilder Output = new StringBuilder();
			int i = 0;

			foreach (byte b in Data)
			{
				if (i > 0)
					Output.Append(' ');

				Output.Append(b.ToString("X2"));

				i = (i + 1) & 31;
				if (i == 0)
					Output.AppendLine();
			}

			return Output.ToString().TrimEnd();
		}

		public override void ReceiveText(DateTime Timestamp, string Text)
		{
			this.view.Add(new SniffItem(Timestamp, SniffItemType.TextReceived, Text, null, Colors.White, Colors.Navy));
		}

		public override void TransmitText(DateTime Timestamp, string Text)
		{
			this.view.Add(new SniffItem(Timestamp, SniffItemType.TextTransmitted, Text, null, Colors.Black, Colors.White));
		}

		public override void Information(DateTime Timestamp, string Comment)
		{
			this.view.Add(new SniffItem(Timestamp, SniffItemType.Information, Comment, null, Colors.Yellow, Colors.DarkGreen));
		}

		public override void Warning(DateTime Timestamp, string Warning)
		{
			this.view.Add(new SniffItem(Timestamp, SniffItemType.Warning, Warning, null, Colors.Black, Colors.Yellow));
		}

		public override void Error(DateTime Timestamp, string Error)
		{
			this.view.Add(new SniffItem(Timestamp, SniffItemType.Error, Error, null, Colors.White, Colors.Red));
		}

		public override void Exception(DateTime Timestamp, string Exception)
		{
			this.view.Add(new SniffItem(Timestamp, SniffItemType.Exception, Exception, null, Colors.White, Colors.DarkRed));
		}
	}
}
