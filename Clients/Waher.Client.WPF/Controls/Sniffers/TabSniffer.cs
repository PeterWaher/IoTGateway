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
	public class TabSniffer : ISniffer
	{
		private TabItem tabItem;
		private SnifferView view;
		private string snifferId = null;

		public TabSniffer(TabItem TabItem, SnifferView View)
		{
			this.tabItem = TabItem;
			this.view = View;
		}

		public string SnifferId
		{
			get { return this.snifferId; }
			set { this.snifferId = value; }
		}

		public void ReceiveBinary(byte[] Data)
		{
			this.view.Add(new SniffItem(SniffItemType.DataReceived, HexToString(Data), Data, Colors.White, Colors.Navy));
		}

		public void TransmitBinary(byte[] Data)
		{
			this.view.Add(new SniffItem(SniffItemType.DataTransmitted, HexToString(Data), Data, Colors.Black, Colors.White));
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

		public void ReceiveText(string Text)
		{
			this.view.Add(new SniffItem(SniffItemType.TextReceived, Text, null, Colors.White, Colors.Navy));
		}

		public void TransmitText(string Text)
		{
			this.view.Add(new SniffItem(SniffItemType.TextTransmitted, Text, null, Colors.Black, Colors.White));
		}

		public void Information(string Comment)
		{
			this.view.Add(new SniffItem(SniffItemType.Information, Comment, null, Colors.Yellow, Colors.DarkGreen));
		}

		public void Warning(string Warning)
		{
			this.view.Add(new SniffItem(SniffItemType.Warning, Warning, null, Colors.Black, Colors.Yellow));
		}

		public void Error(string Error)
		{
			this.view.Add(new SniffItem(SniffItemType.Error, Error, null, Colors.White, Colors.Red));
		}

		public void Exception(string Exception)
		{
			this.view.Add(new SniffItem(SniffItemType.Exception, Exception, null, Colors.White, Colors.DarkRed));
		}
	}
}
