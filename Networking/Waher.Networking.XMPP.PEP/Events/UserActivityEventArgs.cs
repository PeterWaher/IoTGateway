namespace Waher.Networking.XMPP.PEP.Events
{
	/// <summary>
	/// Event arguments for user activity events.
	/// </summary>
	public class UserActivityEventArgs : PersonalEventNotificationEventArgs
	{
		private readonly UserActivity activity;

		internal UserActivityEventArgs(UserActivity Activity, PersonalEventNotificationEventArgs e):
			base(e)
		{
			this.activity = Activity;
		}

		/// <summary>
		/// User activity.
		/// </summary>
		public UserActivity Activity => this.activity;
    }
}
