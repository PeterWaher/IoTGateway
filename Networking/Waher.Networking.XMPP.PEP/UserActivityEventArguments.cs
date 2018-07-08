using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// Delegate for user activity events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Activity"></param>
	public delegate void UserActivityEventHandler(object Sender, UserActivityEventArguments Activity);

	/// <summary>
	/// Event arguments for user activity events.
	/// </summary>
	public class UserActivityEventArguments : PersonalEventNotificationEventArgs
	{
		private UserActivity activity;

		internal UserActivityEventArguments(UserActivity Activity, PersonalEventNotificationEventArgs e):
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
