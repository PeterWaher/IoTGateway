using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Delegate for IsFriend callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments.</param>
	public delegate void IsFriendEventHandler(object Sender, IsFriendEventArgs e);

	/// <summary>
	/// Event arguments for IsFriend events.
	/// </summary>
	public class IsFriendEventArgs : QuestionEventArgs
	{
		/// <summary>
		/// Event arguments for IsFriend events.
		/// </summary>
		/// <param name="Client">XMPP Client used.</param>
		/// <param name="e">Message with request.</param>
		public IsFriendEventArgs(XmppClient Client, MessageEventArgs e)
			: base(Client, e)
		{
		}

		/// <summary>
		/// Accepts the friendship.
		/// </summary>
		/// <param name="Callback">Callback method to call when response is received.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the response could be sent.</returns>
		public bool Accept(IqResultEventHandler Callback, object State)
		{
			return this.Respond(true, Callback, State);
		}

		/// <summary>
		/// Rejects the friendship.
		/// </summary>
		/// <returns>If the response could be sent.</returns>
		public bool Reject(IqResultEventHandler Callback, object State)
		{
			return this.Respond(false, Callback, State);
		}

		private bool Respond(bool IsFriend, IqResultEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<isFriendRule xmlns='");
			Xml.Append(ProvisioningClient.NamespaceProvisioningOwner);
			Xml.Append("' jid='");
			Xml.Append(XML.Encode(this.JID));
			Xml.Append("' remoteJid='");
			Xml.Append(XML.Encode(this.RemoteJID));
			Xml.Append("' key='");
			Xml.Append(XML.Encode(this.Key));
			Xml.Append("' result='");
			Xml.Append(CommonTypes.Encode(IsFriend));
			Xml.Append("'/>");

			RosterItem Item = this.Client[this.FromBareJID];
			if (Item.HasLastPresence && Item.LastPresence.IsOnline)
			{
				this.Client.SendIqSet(Item.LastPresenceFullJid, Xml.ToString(), Callback, State);
				return true;
			}
			else
				return false;
		}

	}
}
