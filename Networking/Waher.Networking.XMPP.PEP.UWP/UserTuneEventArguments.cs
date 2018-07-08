using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// Delegate for user tune events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Tune"></param>
	public delegate void UserTuneEventHandler(object Sender, UserTuneEventArguments Tune);

	/// <summary>
	/// Event arguments for user tune events.
	/// </summary>
	public class UserTuneEventArguments : PersonalEventNotificationEventArgs
	{
		private UserTune tune;

		internal UserTuneEventArguments(UserTune Tune, PersonalEventNotificationEventArgs e):
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
