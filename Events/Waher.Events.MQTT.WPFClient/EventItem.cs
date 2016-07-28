using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Waher.Events;

namespace Waher.Events.MQTT.WPFClient
{
	/// <summary>
	/// Represents one item in a event log.
	/// </summary>
	public class EventItem
	{
		private Color foregroundColor;
		private Color backgroundColor;
		private Event e;
		private bool selected = false;

		/// <summary>
		/// Represents one item in a event log.
		/// </summary>
		/// <param name="Event">Event</param>
		public EventItem(Event Event)
			: base()
		{
			this.e = Event;

			switch (Event.Type)
			{
				case EventType.Debug:
					this.foregroundColor = Colors.White;
					this.backgroundColor = Colors.DarkBlue;
					break;

				case EventType.Informational:
					this.foregroundColor = Colors.Black;
					this.backgroundColor = Colors.White;
					break;

				case EventType.Notice:
					this.foregroundColor = Colors.Black;
					this.backgroundColor = Colors.LightYellow;
					break;

				case EventType.Warning:
					this.foregroundColor = Colors.Black;
					this.backgroundColor = Colors.Yellow;
					break;

				case EventType.Error:
					this.foregroundColor = Colors.Yellow;
					this.backgroundColor = Colors.Red;
					break;

				case EventType.Critical:
					this.foregroundColor = Colors.Yellow;
					this.backgroundColor = Colors.Crimson;
					break;

				case EventType.Alert:
					this.foregroundColor = Colors.Yellow;
					this.backgroundColor = Colors.DarkRed;
					break;

				case EventType.Emergency:
					this.foregroundColor = Colors.White;
					this.backgroundColor = Colors.Magenta;
					break;

				default:
					this.foregroundColor = Colors.Black;
					this.backgroundColor = Colors.White;
					break;
			}
		}

		protected void Raise(EventHandler h)
		{
			if (h != null)
			{
				try
				{
					h(this, new EventArgs());
				}
				catch (Exception ex)
				{
					MessageBox.Show(MainWindow.currentInstance, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		/// <summary>
		/// If the node is selected.
		/// </summary>
		public bool IsSelected
		{
			get { return this.selected; }
			set
			{
				if (this.selected != value)
				{
					this.selected = value;

					if (this.selected)
						this.OnSelected();
					else
						this.OnDeselected();
				}
			}
		}

		/// <summary>
		/// Event raised when the node has been selected.
		/// </summary>
		public event EventHandler Selected = null;

		/// <summary>
		/// Event raised when the node has been deselected.
		/// </summary>
		public event EventHandler Deselected = null;

		/// <summary>
		/// Raises the <see cref="Selected"/> event.
		/// </summary>
		protected virtual void OnSelected()
		{
			this.Raise(this.Selected);
		}

		/// <summary>
		/// Raises the <see cref="Deselected"/> event.
		/// </summary>
		protected virtual void OnDeselected()
		{
			this.Raise(this.Deselected);
		}

		/// <summary>
		/// Foreground color
		/// </summary>
		public Color ForegroundColor
		{
			get { return this.foregroundColor; }
			set { this.foregroundColor = value; }
		}

		/// <summary>
		/// Foreground color, as a string
		/// </summary>
		public string ForegroundColorString
		{
			get { return this.foregroundColor.ToString(); }
		}

		/// <summary>
		/// Background color
		/// </summary>
		public Color BackgroundColor
		{
			get { return this.backgroundColor; }
			set { this.backgroundColor = value; }
		}

		/// <summary>
		/// Background color, as a string
		/// </summary>
		public string BackgroundColorString
		{
			get { return this.backgroundColor.ToString(); }
		}

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public DateTime Timestamp { get { return this.e.Timestamp; } }

		/// <summary>
		/// Type of event.
		/// </summary>
		public EventType Type { get { return this.e.Type; } }

		/// <summary>
		/// Event Level.
		/// </summary>
		public EventLevel Level { get { return this.e.Level; } }

		/// <summary>
		/// Free-text event message.
		/// </summary>
		public string Message { get { return this.e.Message; } }

		/// <summary>
		/// Object related to the event.
		/// </summary>
		public string Object { get { return this.e.Object; } }

		/// <summary>
		/// Actor responsible for the action causing the event.
		/// </summary>
		public string Actor { get { return this.e.Actor; } }

		/// <summary>
		/// Computer-readable Event ID identifying type of even.
		/// </summary>
		public string EventId { get { return this.e.EventId; } }

		/// <summary>
		/// Facility can be either a facility in the network sense or in the system sense.
		/// </summary>
		public string Facility { get { return this.e.Facility; } }

		/// <summary>
		/// Module where the event is reported.
		/// </summary>
		public string Module { get { return this.e.Module; } }

		/// <summary>
		/// Stack Trace of event.
		/// </summary>
		public string StackTrace { get { return this.e.StackTrace; } }

		/// <summary>
		/// Variable set of tags providing event-specific information.
		/// </summary>
		public KeyValuePair<string, object>[] Tags { get { return this.e.Tags; } }

	}
}
