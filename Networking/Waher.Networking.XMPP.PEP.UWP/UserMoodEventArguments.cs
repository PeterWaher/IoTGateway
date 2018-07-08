using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// Delegate for user mood events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Mood"></param>
	public delegate void UserMoodEventHandler(object Sender, UserMoodEventArguments Mood);

	/// <summary>
	/// Event arguments for user mood events.
	/// </summary>
	public class UserMoodEventArguments : PersonalEventNotificationEventArgs
	{
		private UserMood mood;

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
