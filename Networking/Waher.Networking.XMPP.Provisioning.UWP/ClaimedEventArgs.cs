using System;
using Waher.Things;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event argument base class for node information and JID events.
	/// </summary>
	public class ClaimedEventArgs : NodeJidEventArgs
	{
		private readonly bool isPublic;

		internal ClaimedEventArgs(IqEventArgs e, ThingReference Node, string Jid, bool Public)
			: base(e, Node, Jid)
		{
			this.isPublic = Public;
		}

		/// <summary>
		/// If the device is considered a public device, meaning it's available in searches in the thing registry.
		/// </summary>
		public bool IsPublic
		{
			get { return this.isPublic; }
		}
	}
}
