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
	/// <summary>
	/// Sniffer displaying incoming events in a ListView component.
	/// </summary>
	public class ListViewSniffer : ISniffer
	{
		private ListView listView;
		private int maxItems = 1000;

		/// <summary>
		/// Sniffer displaying incoming events in a ListView component.
		/// </summary>
		/// <param name="ListView">Component receiving incoming events.</param>
		public ListViewSniffer(ListView ListView)
		{
			this.listView = ListView;
		}

		/// <summary>
		/// Component receiving incoming events.
		/// </summary>
		public ListView ListView
		{
			get { return this.listView; }
		}

		/// <summary>
		/// Maximum number of items in the list view.
		/// </summary>
		public int MaxItems
		{
			get { return this.maxItems; }
			set { this.maxItems = value; }
		}

		private async void Add(SniffItem SniffItem)
		{
			await this.listView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
			{
				int c;

				this.listView.Items.Insert(0, SniffItem);

				c = this.listView.Items.Count;
				while (c > this.maxItems)
					this.listView.Items.RemoveAt(--c);
			});
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(byte[] Data)
		{
			this.Add(new SniffItem(SniffItemType.DataReceived, HexToString(Data), Data, Colors.White, Colors.Navy));
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
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

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text)
		{
			this.Add(new SniffItem(SniffItemType.TextReceived, Text, null, Colors.White, Colors.Navy));
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text)
		{
			this.Add(new SniffItem(SniffItemType.TextTransmitted, Text, null, Colors.Black, Colors.White));
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment)
		{
			this.Add(new SniffItem(SniffItemType.Information, Comment, null, Colors.Yellow, Colors.DarkGreen));
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning)
		{
			this.Add(new SniffItem(SniffItemType.Warning, Warning, null, Colors.Black, Colors.Yellow));
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error)
		{
			this.Add(new SniffItem(SniffItemType.Error, Error, null, Colors.White, Colors.Red));
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Exception)
		{
			this.Add(new SniffItem(SniffItemType.Exception, Exception, null, Colors.White, Colors.DarkRed));
		}
	}
}
