using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// 
	/// </summary>
	public enum E2ETransmission
	{
		/// <summary>
		/// Will only send the stanza, if end-to-end encryption can be used. If not, an exception is thrown.
		/// </summary>
		AssertE2E,

		/// <summary>
		/// Will only send the stanza, if end-to-end encryption can be used. If not, the stanza is ignored.
		/// </summary>
		IgnoreIfNotE2E,

		/// <summary>
		/// Will send the stanza using end-to-end encryption, if possible. If not, the stanza is sent as a normal
		/// stanza.
		/// </summary>
		NormalIfNotE2E
	}

	/// <summary>
	/// End-to-end encryption interface.
	/// </summary>
	public interface IEndToEndEncryption : IDisposable
	{
		/// <summary>
		/// Encrypts binary data that can be sent to an XMPP client out of band.
		/// </summary>
		/// <param name="BareJid">Bare JID of recipient.</param>
		/// <param name="Data">Data to encrypt.</param>
		/// <returns>Encrypted data, if encryption was possible to the recipient, or null if not.</returns>
		byte[] Encrypt(string BareJid, byte[] Data);

		/// <summary>
		/// Decrypts binary data received from an XMPP client out of band.
		/// </summary>
		/// <param name="BareJid">Bare JID of sender.</param>
		/// <param name="Data">Data to decrypt.</param>
		/// <returns>Decrypted data, if decryption was possible from the recipient, or null if not.</returns>
		byte[] Decrypt(string BareJid, byte[] Data);

		/// <summary>
		/// Encrypts data into XML that can be sent over XMPP.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted stanza through.</param>
		/// <param name="BareJid">Bare JID of recipient.</param>
		/// <param name="DataXml">Data to encrypt.</param>
		/// <param name="Xml">XML containing the encrypted data will be output here.</param>
		/// <returns>If encryption was possible to the recipient, or not.</returns>
		bool Encrypt(XmppClient Client, string BareJid, string DataXml, StringBuilder Xml);

		/// <summary>
		/// Sends an end-to-end encrypted message, if possible. If recipient does not support end-to-end
		/// encryption, the message is sent normally.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted stanza through.</param>
		/// <param name="E2ETransmission">How transmission is to be handled if no end-to-end encryption
		/// can be performed.</param>
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
		void SendMessage(XmppClient Client, E2ETransmission E2ETransmission, QoSLevel QoS, MessageType Type, 
			string Id, string To, string CustomXml, string Body, string Subject, string Language, string ThreadId,
			string ParentThreadId, DeliveryEventHandler DeliveryCallback, object State);

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted stanza through.</param>
		/// <param name="E2ETransmission">How transmission is to be handled if no end-to-end encryption
		/// can be performed.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>ID of IQ stanza</returns>
		uint SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml, 
			IqResultEventHandler Callback, object State);

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted stanza through.</param>
		/// <param name="E2ETransmission">How transmission is to be handled if no end-to-end encryption
		/// can be performed.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <returns>ID of IQ stanza</returns>
		uint SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml, 
			IqResultEventHandler Callback, object State, int RetryTimeout, int NrRetries);

		/// <summary>
		/// Sends an IQ Get request.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted stanza through.</param>
		/// <param name="E2ETransmission">How transmission is to be handled if no end-to-end encryption
		/// can be performed.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		/// <returns>ID of IQ stanza</returns>
		uint SendIqGet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml, 
			IqResultEventHandler Callback, object State, int RetryTimeout, int NrRetries, bool DropOff, 
			int MaxRetryTimeout);

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted stanza through.</param>
		/// <param name="E2ETransmission">How transmission is to be handled if no end-to-end encryption
		/// can be performed.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>ID of IQ stanza</returns>
		uint SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml, 
			IqResultEventHandler Callback, object State);

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted stanza through.</param>
		/// <param name="E2ETransmission">How transmission is to be handled if no end-to-end encryption
		/// can be performed.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <returns>ID of IQ stanza</returns>
		uint SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml, 
			IqResultEventHandler Callback, object State, int RetryTimeout, int NrRetries);

		/// <summary>
		/// Sends an IQ Set request.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted stanza through.</param>
		/// <param name="E2ETransmission">How transmission is to be handled if no end-to-end encryption
		/// can be performed.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="RetryTimeout">Retry Timeout, in milliseconds.</param>
		/// <param name="NrRetries">Number of retries.</param>
		/// <param name="DropOff">If the retry timeout should be doubled between retries (true), or if the same retry timeout 
		/// should be used for all retries. The retry timeout will never exceed <paramref name="MaxRetryTimeout"/>.</param>
		/// <param name="MaxRetryTimeout">Maximum retry timeout. Used if <paramref name="DropOff"/> is true.</param>
		/// <returns>ID of IQ stanza</returns>
		uint SendIqSet(XmppClient Client, E2ETransmission E2ETransmission, string To, string Xml, 
			IqResultEventHandler Callback, object State, int RetryTimeout, int NrRetries, bool DropOff, 
			int MaxRetryTimeout);

		/// <summary>
		/// Returns a response to an IQ Get/Set request.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted stanza through.</param>
		/// <param name="E2ETransmission">How transmission is to be handled if no end-to-end encryption
		/// can be performed.</param>
		/// <param name="Id">ID attribute of original IQ request.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the response.</param>
		void SendIqResult(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To, string Xml);

		/// <summary>
		/// Returns an error response to an IQ Get/Set request.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted stanza through.</param>
		/// <param name="E2ETransmission">How transmission is to be handled if no end-to-end encryption
		/// can be performed.</param>
		/// <param name="Id">ID attribute of original IQ request.</param>
		/// <param name="To">Destination address</param>
		/// <param name="Xml">XML to embed into the response.</param>
		void SendIqError(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To, string Xml);

		/// <summary>
		/// Returns an error response to an IQ Get/Set request.
		/// </summary>
		/// <param name="Client">XMPP client to send the end-to-end encrypted stanza through.</param>
		/// <param name="E2ETransmission">How transmission is to be handled if no end-to-end encryption
		/// can be performed.</param>
		/// <param name="Id">ID attribute of original IQ request.</param>
		/// <param name="To">Destination address</param>
		/// <param name="ex">Internal exception object.</param>
		void SendIqError(XmppClient Client, E2ETransmission E2ETransmission, string Id, string To, Exception ex);

	}
}
