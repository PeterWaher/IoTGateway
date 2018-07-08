using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Networking.XMPP.PubSub;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// Delegate for personal event notificaction event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments</param>
	public delegate void PersonalEventNotificationEventHandler(object Sender, PersonalEventNotificationEventArgs e);

	/// <summary>
	/// Event argument for personal event notification events.
	/// </summary>
	public class PersonalEventNotificationEventArgs : ItemNotificationEventArgs
    {
		private readonly IPersonalEvent personalEvent;

		/// <summary>
		/// Event argument for personal event notification events.
		/// </summary>
		/// <param name="PersonalEvent">Personal event</param>
		/// <param name="e">Message event arguments</param>
		public PersonalEventNotificationEventArgs(IPersonalEvent PersonalEvent, ItemNotificationEventArgs e)
			: base(e)
		{
			this.personalEvent = PersonalEvent;
		}

		/// <summary>
		/// Event argument for personal event notification events.
		/// </summary>
		/// <param name="PersonalEvent">Personal event</param>
		/// <param name="e">Message event arguments</param>
		public PersonalEventNotificationEventArgs(PersonalEventNotificationEventArgs e)
			: base(e)
		{
			this.personalEvent = e.personalEvent;
		}

		/// <summary>
		/// Parsed personal event, if appropriate type was found.
		/// </summary>
		public IPersonalEvent PersonalEvent
		{
			get { return this.personalEvent; }
		}
	}
}
