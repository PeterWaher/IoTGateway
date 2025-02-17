using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.IoTGateway.Setup.Windows
{
	/// <summary>
	/// Channels logged events to the main window.
	/// </summary>
	public class MainWindowEventSink : EventSink
	{
		private readonly MainWindow window;
		private LinkedList<Event>? queue = new();

		/// <summary>
		/// Channels logged events to the main window.
		/// </summary>
		/// <param name="Window">Window reference.</param>
		public MainWindowEventSink(MainWindow Window)
			: base("MainWindow event sink")
		{
			this.window = Window;
		}

		/// <summary>
		/// Processes an event.
		/// </summary>
		/// <param name="Event">Event that has been logged.</param>
		public override Task Queue(Event Event)
		{
			if (this.queue is not null)
				this.queue.AddLast(Event);
			else
			{
				StringBuilder sb = new();

				if (Event.Type != EventType.Informational || Event.Level != EventLevel.Minor)
				{
					sb.Append(Event.Level.ToString());
					sb.Append(' ');
					sb.Append(Event.Type.ToString());
					sb.Append(": ");
				}

				sb.Append(Event.Message);

				Append(sb, "Event ID", Event.EventId);
				Append(sb, "Object", Event.Object);
				Append(sb, "Actor", Event.Actor);
				Append(sb, "Module", Event.Module);
				Append(sb, "Facility", Event.Facility);

				if (Event.Tags is not null)
				{
					foreach (KeyValuePair<string, object> P in Event.Tags)
						Append(sb, P.Key, P.Value?.ToString() ?? string.Empty);
				}

				this.window.Dispatcher.BeginInvoke(() =>
				{
					this.window.AddStatus(Event.Type, sb.ToString());
				});
			}

			return Task.CompletedTask;
		}

		public void Start()
		{
			if (this.queue is not null)
			{
				LinkedList<Event> ToProcess = this.queue;
				this.queue = null;

				foreach (Event Event in ToProcess)
					this.Queue(Event);
			}
		}

		private static void Append(StringBuilder sb, string Label, string Value)
		{
			if (!string.IsNullOrEmpty(Value))
			{
				sb.Append(" (");
				sb.Append(Label);
				sb.Append(": ");
				sb.Append(Value);
				sb.Append(')');
			}
		}

	}
}
