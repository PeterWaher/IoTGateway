using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event arguments for Registration callbacks.
	/// </summary>
	public class RegistrationEventArgs : IqResultEventArgs
	{
		private string ownerJid;

		internal RegistrationEventArgs(IqResultEventArgs e, object State, string OwnerJid)
			: base(e)
		{
			this.State = State;
			this.ownerJid = OwnerJid;
		}

		/// <summary>
		/// Owner JID, if claimed.
		/// </summary>
		public string OwnerJid
		{
			get { return this.ownerJid; }
		}

		/// <summary>
		/// If the device is already claimed.
		/// </summary>
		public bool IsClaimed
		{
			get { return !string.IsNullOrEmpty(this.ownerJid); }
		}
	}
}
