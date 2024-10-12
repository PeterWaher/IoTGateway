using System.Threading.Tasks;
using Waher.Things.Ieee1451.Ieee1451_0.Model;

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
		/// Name of <see cref="Message.NetworkServiceId"/>
		/// </summary>
		public override string NetworkServiceIdName => this.EventNotificationService.ToString();

		/// <summary>
		/// Process incoming message.
		/// </summary>
		/// <param name="Client">Client interface.</param>
		public override Task ProcessIncoming(IClient Client)
		{
			switch (this.MessageType)
			{
				case MessageType.Command:
					return Client.EventNotificationCommand(this);

				case MessageType.Reply:
					return Client.EventNotificationReply(this);

				case MessageType.Announcement:
					return Client.EventNotificationAnnouncement(this);

				case MessageType.Notification:
					return Client.EventNotificationNotification(this);

				case MessageType.Callback:
					return Client.EventNotificationCallback(this);

				default:
					return Task.CompletedTask;
			}
		}
	}
}
