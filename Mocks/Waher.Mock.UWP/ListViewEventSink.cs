using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Waher.Mock
{
	/// <summary>
	/// Event sink displaying incoming events in a ListView component.
	/// </summary>
	public class ListViewEventSink : EventSink
	{
		private ListView listView;
		private int maxItems = 1000;

		/// <summary>
		/// Event sink displaying incoming events in a ListView component.
		/// </summary>
		/// <param name="ObjectID">Object ID.</param>
		/// <param name="ListView">Component receiving logged events.</param>
		public ListViewEventSink(string ObjectID, ListView ListView)
			: base(ObjectID)
		{
			this.listView = ListView;
		}

		/// <summary>
		/// Component receiving logged events.
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
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override Task Queue(Event Event)
		{
			switch (Event.Type)
			{
				case EventType.Debug:
					this.Add(new SniffItem(SniffItemType.Information, Event.Message, null, Colors.White, Colors.Indigo));
					break;

				case EventType.Informational:
					this.Add(new SniffItem(SniffItemType.Information, Event.Message, null, Colors.Black, Colors.White));
					break;

				case EventType.Notice:
					this.Add(new SniffItem(SniffItemType.Information, Event.Message, null, Colors.Black, Colors.LemonChiffon));
					break;

				case EventType.Warning:
					this.Add(new SniffItem(SniffItemType.Information, Event.Message, null, Colors.Black, Colors.Yellow));
					break;

				case EventType.Error:
					this.Add(new SniffItem(SniffItemType.Information, Event.Message, null, Colors.White, Colors.IndianRed));
					break;

				case EventType.Critical:
					this.Add(new SniffItem(SniffItemType.Information, Event.Message, null, Colors.White, Colors.Crimson));
					break;

				case EventType.Alert:
					this.Add(new SniffItem(SniffItemType.Information, Event.Message, null, Colors.White, Colors.Maroon));
					break;

				case EventType.Emergency:
					this.Add(new SniffItem(SniffItemType.Information, Event.Message, null, Colors.White, Colors.Black));
					break;
			}

			return Task.CompletedTask;
		}
	}
}
