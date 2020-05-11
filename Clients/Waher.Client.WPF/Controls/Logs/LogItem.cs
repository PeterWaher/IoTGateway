using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

		/// <summary>
		/// Represents one item in an event log output.
		/// </summary>
		public LogItem(Event Event)
			: base(CalcForegroundColor(Event), CalcBackgroundColor(Event))
		{
			this.e = Event;
		}

		/// <summary>
		/// Event object
		/// </summary>
		public Event Event => this.e;

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public DateTime Timestamp { get { return this.e.Timestamp; } }

		/// <summary>
		/// Event type.
		/// </summary>
		public EventType Type { get { return this.e.Type; } }

		/// <summary>
		/// Event level.
		/// </summary>
		public EventLevel Level { get { return this.e.Level; } }

		/// <summary>
		/// Time of day of event, as a string.
		/// </summary>
		public string Time { get { return this.e.Timestamp.ToLongTimeString(); } }

		/// <summary>
		/// Event ID
		/// </summary>
		public string EventId { get { return this.e.EventId; } }

		/// <summary>
		/// Object
		/// </summary>
		public string Object { get { return this.e.Object; } }

		/// <summary>
		/// Actor
		/// </summary>
		public string Actor { get { return this.e.Actor; } }

		/// <summary>
		/// Message
		/// </summary>
		public string Message { get { return this.e.Message; } }

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
