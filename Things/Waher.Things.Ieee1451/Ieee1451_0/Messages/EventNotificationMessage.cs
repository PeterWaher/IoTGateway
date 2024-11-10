using Waher.Networking;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
    /// <summary>
    /// IEEE 1451.0 Event Notification Message
    /// </summary>
    public class EventNotificationMessage : Message
	{
		/// <summary>
		/// IEEE 1451.0 Event Notification Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="EventNotificationService">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		/// <param name="Sniffable">Sniffable interface on which the message was received.</param>
		public EventNotificationMessage(NetworkServiceType NetworkServiceType, EventNotificationService EventNotificationService, 
			MessageType MessageType, byte[] Body, byte[] Tail, ICommunicationLayer Sniffable)
			: base(NetworkServiceType, (byte)EventNotificationService, MessageType, Body, Tail, Sniffable)
		{
			this.EventNotificationService = EventNotificationService;
		}

		/// <summary>
		/// Event Notification Service
		/// </summary>
		public EventNotificationService EventNotificationService { get; }

		/// <summary>
		/// Name of <see cref="Message.NetworkServiceId"/>
		/// </summary>
		public override string NetworkServiceIdName => this.EventNotificationService.ToString();
	}
}
