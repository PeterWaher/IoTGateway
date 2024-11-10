using Waher.Networking;
using Waher.Networking.MQTT;

namespace Waher.Events.MQTT
{
    /// <summary>
    /// Event arguments for <see cref="MqttEventReceptor.OnEvent"/> events.
    /// </summary>
    public class EventEventArgs : MqttContent
	{
		private readonly Event ev;

		internal EventEventArgs(MqttContent e, Event Event, ICommunicationLayer Sniffable)
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
