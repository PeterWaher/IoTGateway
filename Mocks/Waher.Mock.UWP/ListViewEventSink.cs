using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Waher.Events;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Waher.Mock
{
	public class ListViewEventSink : EventSink
	{
		private ListView listView;
		private int maxItems = 1000;

		public ListViewEventSink(string ObjectID, ListView ListView)
			: base(ObjectID)
		{
			this.listView = ListView;
		}

		public ListView ListView
		{
			get { return this.listView; }
		}

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

		public override void Queue(Event Event)
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
		}
	}
}
