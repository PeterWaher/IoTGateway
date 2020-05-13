using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Emoji;
using Waher.Content.Markdown;
using Waher.Events;

namespace Waher.IoTGateway.Events
{
	/// <summary>
	/// Event sink that forwards alert and emergency messages as notification messages to administrators.
	/// </summary>
	public class AlertNotifier : EventSink
	{
		/// <summary>
		/// Event sink that forwards alert and emergency messages as notification messages to administrators.
		/// </summary>
		/// <param name="ObjectID">Log Object ID</param>
		public AlertNotifier(string ObjectID)
			: base(ObjectID)
		{
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override Task Queue(Event Event)
		{
			switch (Event.Type)
			{
				case EventType.Alert:
				case EventType.Emergency:
					StringBuilder Markdown = new StringBuilder();

					if (Event.Type == EventType.Alert)
						Markdown.AppendLine("Alert");
					else
						Markdown.AppendLine("Emergency");

					Markdown.AppendLine("===============");
					Markdown.AppendLine();
					Markdown.AppendLine(MarkdownDocument.Encode(Event.Message));
					Markdown.AppendLine();
					Markdown.AppendLine("| Information ||");
					Markdown.AppendLine("|:------|:-----|");

					this.AppendLabel("Timestamp", Event.Timestamp.ToShortDateString() + ", " + Event.Timestamp.ToLongTimeString(), Markdown);
					this.AppendLabel("Event ID", Event.EventId, Markdown);
					this.AppendLabel("Actor", Event.Actor, Markdown);
					this.AppendLabel("Object", Event.Object, Markdown);
					this.AppendLabel("Module", Event.Module, Markdown);
					this.AppendLabel("Facility", Event.Facility, Markdown);

					if (!(Event.Tags is null))
					{
						foreach (KeyValuePair<string, object> P in Event.Tags)
							this.AppendLabel(P.Key, P.Value?.ToString(), Markdown);
					}

					Gateway.SendNotification(Markdown.ToString());
					break;
			}

			return Task.CompletedTask;
		}

		private void AppendLabel(string Label, string Value, StringBuilder Markdown)
		{
			bool First = true;

			Markdown.Append("| ");
			Markdown.Append(MarkdownDocument.Encode(Label));
			Markdown.Append(" | ");

			if (Value.Length > 2 && Value.StartsWith(":") && Value.EndsWith(":") &&
				EmojiUtilities.TryGetEmoji(Value.Substring(1, Value.Length - 2), out EmojiInfo _))
			{
				Markdown.Append(Value);
			}
			else
			{
				foreach (string Row in Value.Trim().Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
				{
					if (First)
						First = false;
					else
						Markdown.Append("<br/>");

					Markdown.Append(MarkdownDocument.Encode(Row));
				}
			}

			Markdown.AppendLine(" |");
		}
	}
}
