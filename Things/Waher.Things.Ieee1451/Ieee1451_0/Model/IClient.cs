using System.Threading.Tasks;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;

namespace Waher.Things.Ieee1451.Ieee1451_0.Model
{
	/// <summary>
	/// Client interface for interacting with IEEE 1451.0.
	/// </summary>
	public interface IClient
	{
		/// <summary>
		/// A transducer access command has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task TransducerAccessCommand(TransducerAccessMessage Message);

		/// <summary>
		/// A transducer access reply has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task TransducerAccessReply(TransducerAccessMessage Message);

		/// <summary>
		/// A transducer access announcement has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task TransducerAccessAnnouncement(TransducerAccessMessage Message);

		/// <summary>
		/// A transducer access notification has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task TransducerAccessNotification(TransducerAccessMessage Message);

		/// <summary>
		/// A transducer access callback has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task TransducerAccessCallback(TransducerAccessMessage Message);

		/// <summary>
		/// A TEDS access command has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task TedsAccessCommand(TedsAccessMessage Message);

		/// <summary>
		/// A TEDS access reply has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task TedsAccessReply(TedsAccessMessage Message);

		/// <summary>
		/// A TEDS access announcement has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task TedsAccessAnnouncement(TedsAccessMessage Message);

		/// <summary>
		/// A TEDS access notification has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task TedsAccessNotification(TedsAccessMessage Message);

		/// <summary>
		/// A TEDS access callback has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task TedsAccessCallback(TedsAccessMessage Message);

		/// <summary>
		/// A discovery command has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task DiscoveryCommand(DiscoveryMessage Message);

		/// <summary>
		/// A discovery reply has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task DiscoveryReply(DiscoveryMessage Message);

		/// <summary>
		/// A discovery announcement has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task DiscoveryAnnouncement(DiscoveryMessage Message);

		/// <summary>
		/// A discovery notification has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task DiscoveryNotification(DiscoveryMessage Message);

		/// <summary>
		/// A discovery callback has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task DiscoveryCallback(DiscoveryMessage Message);

		/// <summary>
		/// An Events notification command has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task EventNotificationCommand(EventNotificationMessage Message);

		/// <summary>
		/// An Events notification reply has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task EventNotificationReply(EventNotificationMessage Message);

		/// <summary>
		/// An Events notification announcement has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task EventNotificationAnnouncement(EventNotificationMessage Message);

		/// <summary>
		/// An Events notification has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task EventNotificationNotification(EventNotificationMessage Message);

		/// <summary>
		/// An Events notification callback has been received.
		/// </summary>
		/// <param name="Message">Message</param>
		Task EventNotificationCallback(EventNotificationMessage Message);
	}
}
