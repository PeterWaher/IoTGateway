namespace Waher.Networking.XMPP.PEP.Events
{
	/// <summary>
	/// Event arguments for user tune events.
	/// </summary>
	public class UserTuneEventArgs : PersonalEventNotificationEventArgs
	{
		private readonly UserTune tune;

		internal UserTuneEventArgs(UserTune Tune, PersonalEventNotificationEventArgs e):
			base(e)
		{
			this.tune = Tune;
		}

		/// <summary>
		/// User tune.
		/// </summary>
		public UserTune Tune => this.tune;
    }
}
