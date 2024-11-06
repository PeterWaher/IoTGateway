using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;

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
		private readonly Event ev;

		internal EventEventArgs(MqttContent e, Event Event, ISniffable Sniffable)
			: base(e.Header, e.Topic, e.Data, Sniffable)
		{
			this.ev = Event;
		}

		/// <summary>
		/// Event.
		/// </summary>
		public Event Event => this.ev;
	}
}
