namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Event Notification Message
	/// </summary>
	public class EventNotificationMessage : Ieee14510Message
	{
		/// <summary>
		/// IEEE 1451.0 Event Notification Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="EventNotificationService">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public EventNotificationMessage(NetworkServiceType NetworkServiceType, EventNotificationService EventNotificationService, 
			MessageType MessageType, byte[] Body, byte[] Tail)
			: base(NetworkServiceType, (byte)EventNotificationService, MessageType, Body, Tail)
		{
			this.EventNotificationService = EventNotificationService;
		}

		/// <summary>
		/// Event Notification Service
		/// </summary>
		public EventNotificationService EventNotificationService { get; }

		/// <summary>
		/// Name of <see cref="Ieee14510Message.NetworkServiceId"/>
		/// </summary>
		public override string NetworkServiceIdName => this.EventNotificationService.ToString();
	}
}
