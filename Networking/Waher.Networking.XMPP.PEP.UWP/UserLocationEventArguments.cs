using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// Delegate for user location events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Location"></param>
	public delegate Task UserLocationEventHandler(object Sender, UserLocationEventArguments Location);

	/// <summary>
	/// Event arguments for user location events.
	/// </summary>
	public class UserLocationEventArguments : PersonalEventNotificationEventArgs
	{
		private readonly UserLocation location;

		internal UserLocationEventArguments(UserLocation Location, PersonalEventNotificationEventArgs e):
			base(e)
		{
			this.location = Location;
		}

		/// <summary>
		/// User location.
		/// </summary>
		public UserLocation Location => this.location;
    }
}
