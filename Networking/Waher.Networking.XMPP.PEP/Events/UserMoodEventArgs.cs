namespace Waher.Networking.XMPP.PEP.Events
{
	/// <summary>
	/// Event arguments for user mood events.
	/// </summary>
	public class UserMoodEventArgs : PersonalEventNotificationEventArgs
	{
		private readonly UserMood mood;

		internal UserMoodEventArgs(UserMood Mood, PersonalEventNotificationEventArgs e):
			base(e)
		{
			this.mood = Mood;
		}

		/// <summary>
		/// User mood.
		/// </summary>
		public UserMood Mood => this.mood;
    }
}
