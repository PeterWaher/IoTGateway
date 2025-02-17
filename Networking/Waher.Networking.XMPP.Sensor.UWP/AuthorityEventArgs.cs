using Waher.Things;

namespace Waher.Networking.XMPP.Events
{
	/// <summary>
	/// Event arguments for authority events, to associate a Bare JID with
	/// an authority that can authorize privileged requests.
	/// </summary>
	public class AuthorityEventArgs
	{
		/// <summary>
		/// Event arguments for authority events, to associate a Bare JID with
		/// an authority that can authorize privileged requests.
		/// </summary>
		public AuthorityEventArgs(string BareJid)
		{
			this.BareJid = BareJid;
		}

		/// <summary>
		/// Bare JID
		/// </summary>
		public string BareJid { get; }

		/// <summary>
		/// Authority associated with Bare JID
		/// </summary>
		public IRequestOrigin Authority { get; set; }
	}
}
