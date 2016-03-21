using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Waher.Networking.Sniffers;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Waher.Mock
{
	public class ListViewSniffer : ISniffer
	{
		private ListView listView;

		public ListViewSniffer(ListView ListView)
		{
			this.listView = ListView;
		}

		public ListView ListView
		{
			get { return this.listView; }
		}

		private async void Add(SniffItem SniffItem)
		{
			await this.listView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => this.listView.Items.Add(SniffItem));
		}

		public void ReceiveBinary(byte[] Data)
		{
			this.Add(new SniffItem(SniffItemType.DataReceived, HexToString(Data), Data, Colors.White, Colors.Navy));
		}

		public void TransmitBinary(byte[] Data)
		{
			this.Add(new SniffItem(SniffItemType.DataTransmitted, HexToString(Data), Data, Colors.Black, Colors.White));
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
			this.Add(new SniffItem(SniffItemType.TextReceived, Text, null, Colors.White, Colors.Navy));
		}

		public void TransmitText(string Text)
		{
			this.Add(new SniffItem(SniffItemType.TextTransmitted, Text, null, Colors.Black, Colors.White));
		}

		public void Information(string Comment)
		{
			this.Add(new SniffItem(SniffItemType.Information, Comment, null, Colors.Yellow, Colors.DarkGreen));
		}

		public void Warning(string Warning)
		{
			this.Add(new SniffItem(SniffItemType.Warning, Warning, null, Colors.Black, Colors.Yellow));
		}

		public void Error(string Error)
		{
			this.Add(new SniffItem(SniffItemType.Error, Error, null, Colors.White, Colors.Red));
		}

		public void Exception(string Exception)
		{
			this.Add(new SniffItem(SniffItemType.Exception, Exception, null, Colors.White, Colors.DarkRed));
		}
	}
}
