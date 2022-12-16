using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;
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
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public override Task ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			return this.Add(new SniffItem(Timestamp, SniffItemType.DataReceived, HexToString(Data), Data, Colors.White, Colors.Navy));
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public override Task TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			return this.Add(new SniffItem(Timestamp, SniffItemType.DataTransmitted, HexToString(Data), Data, Colors.Black, Colors.White));
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
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override Task ReceiveText(DateTime Timestamp, string Text)
		{
			return this.Add(new SniffItem(Timestamp, SniffItemType.TextReceived, Text, null, Colors.White, Colors.Navy));
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override Task TransmitText(DateTime Timestamp, string Text)
		{
			return this.Add(new SniffItem(Timestamp, SniffItemType.TextTransmitted, Text, null, Colors.Black, Colors.White));
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public override Task Information(DateTime Timestamp, string Comment)
		{
			return this.Add(new SniffItem(Timestamp, SniffItemType.Information, Comment, null, Colors.Yellow, Colors.DarkGreen));
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public override Task Warning(DateTime Timestamp, string Warning)
		{
			return this.Add(new SniffItem(Timestamp, SniffItemType.Warning, Warning, null, Colors.Black, Colors.Yellow));
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public override Task Error(DateTime Timestamp, string Error)
		{
			return this.Add(new SniffItem(Timestamp, SniffItemType.Error, Error, null, Colors.White, Colors.Red));
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public override Task Exception(DateTime Timestamp, string Exception)
		{
			return this.Add(new SniffItem(Timestamp, SniffItemType.Exception, Exception, null, Colors.White, Colors.DarkRed));
		}
	}
}
