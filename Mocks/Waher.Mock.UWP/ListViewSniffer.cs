using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;
using Waher.Networking.Sniffers.Model;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Waher.Mock
{
	/// <summary>
	/// Sniffer displaying incoming events in a ListView component.
	/// </summary>
	public class ListViewSniffer : SnifferBase
	{
		private readonly ListView listView;
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
		public ListView ListView => this.listView;

		/// <summary>
		/// How the sniffer handles binary data.
		/// </summary>
		public override BinaryPresentationMethod BinaryPresentationMethod => BinaryPresentationMethod.Hexadecimal;

		/// <summary>
		/// Maximum number of items in the list view.
		/// </summary>
		public int MaxItems
		{
			get => this.maxItems;
			set => this.maxItems = value;
		}

		private async Task Add(SniffItem SniffItem)
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
		/// Processes a binary reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferRxBinary Event)
		{
			return this.Add(new SniffItem(Event.Timestamp, SniffItemType.DataReceived, 
				HexToString(Event.Data, Event.Offset, Event.Count), 
				CloneSection(Event.Data, Event.Offset, Event.Count), Colors.White, Colors.Navy));
		}

		/// <summary>
		/// Processes a binary transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferTxBinary Event)
		{
			return this.Add(new SniffItem(Event.Timestamp, SniffItemType.DataTransmitted, 
				HexToString(Event.Data, Event.Offset, Event.Count), 
				CloneSection(Event.Data, Event.Offset, Event.Count), Colors.Black, Colors.White));
		}

		internal static string HexToString(byte[] Data, int Offset, int Count)
		{
			if (Data is null)
				return "<" + Count.ToString() + " bytes>";
			else
			{
				StringBuilder Output = new StringBuilder();
				int i = 0;
				byte b;

				while (Count > 0)
				{
					b = Data[Offset++];

					if (i > 0)
						Output.Append(' ');

					Output.Append(b.ToString("X2"));

					i = (i + 1) & 31;
					if (i == 0)
						Output.AppendLine();
				}

				return Output.ToString().TrimEnd();
			}
		}

		/// <summary>
		/// Processes a text reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferRxText Event)
		{
			return this.Add(new SniffItem(Event.Timestamp, SniffItemType.TextReceived, Event.Text, null, Colors.White, Colors.Navy));
		}

		/// <summary>
		/// Processes a text transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferTxText Event)
		{
			return this.Add(new SniffItem(Event.Timestamp, SniffItemType.TextTransmitted, Event.Text, null, Colors.Black, Colors.White));
		}

		/// <summary>
		/// Processes an information event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferInformation Event)
		{
			return this.Add(new SniffItem(Event.Timestamp, SniffItemType.Information, Event.Text, null, Colors.Yellow, Colors.DarkGreen));
		}

		/// <summary>
		/// Processes a warning event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferWarning Event)
		{
			return this.Add(new SniffItem(Event.Timestamp, SniffItemType.Warning, Event.Text, null, Colors.Black, Colors.Yellow));
		}

		/// <summary>
		/// Processes an error event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferError Event)
		{
			return this.Add(new SniffItem(Event.Timestamp, SniffItemType.Error, Event.Text, null, Colors.White, Colors.Red));
		}

		/// <summary>
		/// Processes an exception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferException Event)
		{
			return this.Add(new SniffItem(Event.Timestamp, SniffItemType.Exception, Event.Text, null, Colors.White, Colors.DarkRed));
		}
	}
}
