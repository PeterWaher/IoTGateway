using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// End-to-end encryption interface.
	/// </summary>
	public interface IEndToEndEncryption : IDisposable
	{
		/// <summary>
		/// Encrypts data into XML that can be sent over XMPP.
		/// </summary>
		/// <param name="BareJid">Bare JID of recipient.</param>
		/// <param name="Data">Data to encrypt.</param>
		/// <param name="Xml">XML containing the encrypted data will be output here.</param>
		/// <returns>If encryption was possible to the recipient, or not.</returns>
		bool Encrypt(string BareJid, byte[] Data, StringBuilder Xml);

		/// <summary>
		/// Sends an end-to-end encrypted message, if possible. If recipient does not support end-to-end
		/// encryption, the message is sent normally.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted message on.</param>
		/// <param name="QoS">Quality of Service level of message.</param>
		/// <param name="Type">Type of message to send.</param>
		/// <param name="Id">Message ID</param>
		/// <param name="To">Destination address</param>
		/// <param name="CustomXml">Custom XML</param>
		/// <param name="Body">Body text of chat message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="Language">Language used.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="ParentThreadId">Parent Thread ID</param>
		/// <param name="DeliveryCallback">Callback to call when message has been sent, or failed to be sent.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		void SendMessage(XmppClient Client, QoSLevel QoS, MessageType Type, string Id, string To, string CustomXml,
			string Body, string Subject, string Language, string ThreadId,
			string ParentThreadId, DeliveryEventHandler DeliveryCallback, object State);

	}
}
