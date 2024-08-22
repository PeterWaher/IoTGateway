using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Waher.Client.WPF.Model;
using Waher.Events;

namespace Waher.Client.WPF.Controls.Logs
{
	/// <summary>
	/// Represents one item in an event log output.
	/// </summary>
	public class LogItem : ColorableItem
	{
		private readonly Event e;
		private readonly string message;

		/// <summary>
		/// Represents one item in an event log output.
		/// </summary>
		public LogItem(Event Event)
			: base(CalcForegroundColor(Event), CalcBackgroundColor(Event))
		{
			this.e = Event;

			if ((Event.Tags?.Length ?? 0) == 0)
				this.message = Event.Message;
			else
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(Event.Message);

				foreach (KeyValuePair<string, object> P in Event.Tags)
				{
					sb.AppendLine();
					sb.Append(P.Key);
					sb.Append(" = ");
					sb.Append(P.Value?.ToString() ?? string.Empty);
				}

				this.message = sb.ToString();
			}
		}

		/// <summary>
		/// Event object
		/// </summary>
		public Event Event => this.e;

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public DateTime Timestamp => this.e.Timestamp;

		/// <summary>
		/// Event type.
		/// </summary>
		public EventType Type => this.e.Type;

		/// <summary>
		/// Event level.
		/// </summary>
		public EventLevel Level => this.e.Level;

		/// <summary>
		/// Time of day of event, as a string.
		/// </summary>
		public string Time => this.e.Timestamp.ToLongTimeString();

		/// <summary>
		/// Event ID
		/// </summary>
		public string EventId => this.e.EventId;

		/// <summary>
		/// Object
		/// </summary>
		public string Object => this.e.Object;

		/// <summary>
		/// Actor
		/// </summary>
		public string Actor => this.e.Actor;

		/// <summary>
		/// Message
		/// </summary>
		public string Message => this.message;

		private static Color CalcForegroundColor(Event Event)
		{
			switch (Event.Type)
			{
				case EventType.Debug: return Colors.White;
				case EventType.Informational: return Colors.Black;
				case EventType.Notice: return Colors.Black;
				case EventType.Warning: return Colors.Black;
				case EventType.Error: return Colors.Yellow;
				case EventType.Critical: return Colors.White;
				case EventType.Alert: return Colors.White;
				case EventType.Emergency: return Colors.White;
				default: return Colors.Black;
			}
		}

		private static Color CalcBackgroundColor(Event Event)
		{
			switch (Event.Type)
			{
				case EventType.Debug: return Colors.DarkBlue;
				case EventType.Informational: return Colors.White;
				case EventType.Notice: return Colors.LightYellow;
				case EventType.Warning: return Colors.Yellow;
				case EventType.Error: return Colors.Red;
				case EventType.Critical: return Colors.DarkRed;
				case EventType.Alert: return Colors.Purple;
				case EventType.Emergency: return Colors.Black;
				default: return Colors.White;
			}
		}
	}
}
