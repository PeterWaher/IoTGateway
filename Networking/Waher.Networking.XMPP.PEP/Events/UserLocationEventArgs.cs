namespace Waher.Networking.XMPP.PEP.Events
{
	/// <summary>
	/// Event arguments for user location events.
	/// </summary>
	public class UserLocationEventArgs : PersonalEventNotificationEventArgs
	{
		private readonly UserLocation location;

		internal UserLocationEventArgs(UserLocation Location, PersonalEventNotificationEventArgs e):
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
