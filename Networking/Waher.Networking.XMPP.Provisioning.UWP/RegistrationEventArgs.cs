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
		private bool isPublic;

		internal RegistrationEventArgs(IqResultEventArgs e, object State, string OwnerJid, bool IsPublic)
			: base(e)
		{
			this.State = State;
			this.ownerJid = OwnerJid;
			this.isPublic = IsPublic;
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

		/// <summary>
		/// If the claimed device is public.
		/// </summary>
		public bool IsPublic
		{
			get { return this.isPublic; }
		}
	}
}
