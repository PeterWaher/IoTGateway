﻿using Waher.Networking.XMPP.PubSub.Events;

namespace Waher.Networking.XMPP.PEP.Events
{
	/// <summary>
	/// Event argument for personal event notification events.
	/// </summary>
	public class PersonalEventNotificationEventArgs : ItemNotificationEventArgs
    {
		private readonly IPersonalEvent personalEvent;
		private readonly PepClient pepClient;

		/// <summary>
		/// Event argument for personal event notification events.
		/// </summary>
		/// <param name="PersonalEvent">Personal event</param>
		/// <param name="PepClient">Personal Eventing Protocol (PEP) Client.</param>
		/// <param name="e">Message event arguments</param>
		public PersonalEventNotificationEventArgs(IPersonalEvent PersonalEvent, PepClient PepClient, ItemNotificationEventArgs e)
			: base(e)
		{
			this.personalEvent = PersonalEvent;
			this.pepClient = PepClient;
		}

		/// <summary>
		/// Event argument for personal event notification events.
		/// </summary>
		/// <param name="e">Message event arguments</param>
		public PersonalEventNotificationEventArgs(PersonalEventNotificationEventArgs e)
			: base(e)
		{
			this.personalEvent = e.personalEvent;
			this.pepClient = e.pepClient;
		}

		/// <summary>
		/// Parsed personal event, if appropriate type was found.
		/// </summary>
		public IPersonalEvent PersonalEvent => this.personalEvent;

		/// <summary>
		/// Personal Eventing Protocol (PEP) Client.
		/// </summary>
		public PepClient PepClient => this.pepClient;
	}
}
