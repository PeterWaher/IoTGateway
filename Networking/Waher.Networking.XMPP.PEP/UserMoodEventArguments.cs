using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// Delegate for user mood events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Mood"></param>
	public delegate Task UserMoodEventHandler(object Sender, UserMoodEventArguments Mood);

	/// <summary>
	/// Event arguments for user mood events.
	/// </summary>
	public class UserMoodEventArguments : PersonalEventNotificationEventArgs
	{
		private readonly UserMood mood;

		internal UserMoodEventArguments(UserMood Mood, PersonalEventNotificationEventArgs e):
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
