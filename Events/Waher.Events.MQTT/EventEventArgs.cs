using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.MQTT;

namespace Waher.Events.MQTT
{
	/// <summary>
	/// Delegate for <see cref="MqttEventReceptor.OnEvent"/> events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void EventEventHandler(object Sender, EventEventArgs e);

	/// <summary>
	/// Event arguments for <see cref="MqttEventReceptor.OnEvent"/> events.
	/// </summary>
	public class EventEventArgs : MqttContent
	{
		private Event ev;

		internal EventEventArgs(MqttContent e, Event Event)
			: base(e.Header, e.Topic, e.Data)
		{
			this.ev = Event;
		}

		/// <summary>
		/// Event.
		/// </summary>
		public Event Event { get { return this.ev; } }
	}
}
