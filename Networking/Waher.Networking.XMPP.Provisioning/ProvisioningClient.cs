using System;
using System.Collections.Generic;
using System.Xml;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Waher.Content;
using Waher.Events;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Delegate for Token callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments.</param>
	public delegate void TokenCallback(object Sender, TokenEventArgs e);

	/// <summary>
	/// Delegate for IsFriend callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments.</param>
	public delegate void IsFriendCallback(object Sender, IsFriendEventArgs e);

	/// <summary>
	/// Implements an XMPP provisioning client interface.
	/// 
	/// The interface is defined in XEP-0324:
	/// http://xmpp.org/extensions/xep-0324.html
	/// </summary>
	public class ProvisioningClient : IDisposable
	{
		private Dictionary<string, CertificateUse> certificates = new Dictionary<string, CertificateUse>();
		private XmppClient client;
		private string provisioningServerAddress;

		/// <summary>
		/// urn:xmpp:iot:provisioning
		/// </summary>
		public const string NamespaceProvisioning = "urn:xmpp:iot:provisioning";

		/// <summary>
		/// Implements an XMPP provisioning client interface.
		/// 
		/// The interface is defined in XEP-0324:
		/// http://xmpp.org/extensions/xep-0324.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public ProvisioningClient(XmppClient Client, string ProvisioningServerAddress)
		{
			this.client = Client;
			this.provisioningServerAddress = ProvisioningServerAddress;

			this.client.RegisterIqGetHandler("tokenChallenge", NamespaceProvisioning, this.TokenChallengeHandler, true);

			this.client.RegisterMessageHandler("unfriend", NamespaceProvisioning, this.UnfriendHandler, false);
			this.client.RegisterMessageHandler("friend", NamespaceProvisioning, this.FriendHandler, false);

			this.client.OnPresenceSubscribe += Client_OnPresenceSubscribe;
			this.client.OnPresenceUnsubscribe += Client_OnPresenceUnsubscribe;
		}

		private void Client_OnPresenceUnsubscribe(object Sender, PresenceEventArgs e)
		{
			e.Accept();
		}

		private void Client_OnPresenceSubscribe(object Sender, PresenceEventArgs e)
		{
			if (e.From == this.provisioningServerAddress)
				e.Accept();
			else
				this.IsFriend(XmppClient.GetBareJID(e.From), this.CheckIfFriendCallback, e);
		}

		private void CheckIfFriendCallback(object Sender, IsFriendEventArgs e2)
		{
			PresenceEventArgs e = (PresenceEventArgs)e2.State;

			if (e2.Ok && e2.Friend)
				e.Accept();
			else
				e.Decline();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.UnregisterIqGetHandler("tokenChallenge", NamespaceProvisioning, this.TokenChallengeHandler, true);
		}

		/// <summary>
		/// XMPP Client
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// Provisioning server address.
		/// </summary>
		public string ProvisioningServerAddress
		{
			get { return this.provisioningServerAddress; }
		}

		/// <summary>
		/// Gets a token for a certicate. This token can be used to identify services, devices or users. The provisioning server will 
		/// challenge the request, and may choose to challenge it further when it is used, to make sure the sender is the correct holder
		/// of the private certificate.
		/// </summary>
		/// <param name="Certificate">Private certificate. Only the public part will be sent to the provisioning server. But the private
		/// part is required in order to be able to respond to challenges sent by the provisioning server.</param>
		/// <param name="Callback">Callback method called, when token is available.</param>
		/// <param name="State">State object that will be passed on to the callback method.</param>
		public void GetToken(X509Certificate2 Certificate, TokenCallback Callback, object State)
		{
			if (!Certificate.HasPrivateKey)
				throw new ArgumentException("Certificate must have private key.", "Certificate");

			byte[] Bin = Certificate.Export(X509ContentType.Cert);
			string Base64 = System.Convert.ToBase64String(Bin, Base64FormattingOptions.None);

			this.client.SendIqGet(this.provisioningServerAddress, "<getToken xmlns='urn:xmpp:iot:provisioning'>" + Base64 + "</getToken>",
				this.GetTokenResponse, new object[] { Certificate, Callback, State });
		}

		private void GetTokenResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			X509Certificate2 Certificate = (X509Certificate2)P[0];
			XmlElement E = e.FirstElement;

			if (e.Ok && E != null && E.LocalName == "getTokenChallenge" && E.NamespaceURI == NamespaceProvisioning)
			{
				int SeqNr = XML.Attribute(E, "seqnr", 0);
				string Challenge = E.InnerText;
				byte[] Bin = System.Convert.FromBase64String(Challenge);
				Bin = ((RSACryptoServiceProvider)Certificate.PrivateKey).Decrypt(Bin, false);
				string Response = System.Convert.ToBase64String(Bin, Base64FormattingOptions.None);

				this.client.SendIqGet(this.provisioningServerAddress, "<getTokenChallengeResponse xmlns='urn:xmpp:iot:provisioning' seqnr='" +
					SeqNr.ToString() + "'>" + Response + "</getTokenChallengeResponse>",
					this.GetTokenChallengeResponse, P);
			}
		}

		private void GetTokenChallengeResponse(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			X509Certificate2 Certificate = (X509Certificate2)P[0];
			TokenCallback Callback = (TokenCallback)P[1];
			object State = P[2];
			XmlElement E = e.FirstElement;
			string Token;

			if (e.Ok && E != null && E.LocalName == "getTokenResponse" && E.NamespaceURI == NamespaceProvisioning)
			{
				Token = XML.Attribute(E, "token");

				lock (this.certificates)
				{
					this.certificates[Token] = new CertificateUse(Token, Certificate);
				}
			}
			else
				Token = null;

			TokenEventArgs e2 = new TokenEventArgs(e, State, Token);
			try
			{
				Callback(this, e2);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Tells the client a token has been used, for instance in a sensor data request or control operation. Tokens must be
		/// refreshed when they are used, to make sure the client only responds to challenges of recently used certificates.
		/// </summary>
		/// <param name="Token">Token</param>
		public void TokenUsed(string Token)
		{
			CertificateUse Use;

			lock (this.certificates)
			{
				if (this.certificates.TryGetValue(Token, out Use))
					Use.LastUse = DateTime.Now;
			}
		}

		/// <summary>
		/// Tells the client a token has been used, for instance in a sensor data request or control operation. Tokens must be
		/// refreshed when they are used, to make sure the client only responds to challenges of recently used certificates.
		/// </summary>
		/// <param name="Token">Token</param>
		/// <param name="RemoteJid">Remote JID of entity sending the token.</param>
		public void TokenUsed(string Token, string RemoteJid)
		{
			CertificateUse Use;

			lock (this.certificates)
			{
				if (this.certificates.TryGetValue(Token, out Use))
				{
					Use.LastUse = DateTime.Now;
					Use.RemoteCertificateJid = RemoteJid;
				}
				else
					this.certificates[Token] = new CertificateUse(Token, RemoteJid);
			}
		}

		private void TokenChallengeHandler(object Sender, IqEventArgs e)
		{
			XmlElement E = e.Query;
			string Token = XML.Attribute(E, "token");
			string Challenge = E.InnerText;
			CertificateUse Use;

			lock (this.certificates)
			{
				if (!this.certificates.TryGetValue(Token, out Use) || (DateTime.Now - Use.LastUse).TotalMinutes > 1)
					throw new ForbiddenException("Token not recognized.", e.IQ);
			}

			X509Certificate2 Certificate = Use.LocalCertificate;
			if (Certificate != null)
			{
				byte[] Bin = System.Convert.FromBase64String(Challenge);
				Bin = ((RSACryptoServiceProvider)Certificate.PrivateKey).Decrypt(Bin, false);
				string Response = System.Convert.ToBase64String(Bin, Base64FormattingOptions.None);

				e.IqResult("<tokenChallengeResponse xmlns='" + NamespaceProvisioning + "'>" + Response + "</tokenChallengeResponse>");
			}
			else
				this.client.SendIqGet(Use.RemoteCertificateJid, e.Query.OuterXml, this.ForwardedTokenChallengeResponse, e);
		}

		private void ForwardedTokenChallengeResponse(object Sender, IqResultEventArgs e2)
		{
			IqEventArgs e = (IqEventArgs)e2.State;

			if (e2.Ok)
				e.IqResult(e2.FirstElement.OuterXml);
			else
				e.IqError(e2.ErrorElement.OuterXml);
		}

		public void IsFriend(string JID, IsFriendCallback Callback, object State)
		{
			this.client.SendIqGet(this.provisioningServerAddress, "<isFriend xmlns='" + NamespaceProvisioning + "' jid='" +
				XML.Encode(JID) + "'/>", this.IsFriendCallback, new object[] { Callback, State });
		}

		private void IsFriendCallback(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			IsFriendCallback Callback = (IsFriendCallback)P[0];
			object State = P[1];
			string JID;
			bool Result;
			XmlElement E = e.FirstElement;

			if (e.Ok && E != null && E.LocalName == "isFriendResponse" && E.NamespaceURI == NamespaceProvisioning)
			{
				JID = XML.Attribute(E, "jid");
				Result = XML.Attribute(E, "result", false);
			}
			else
			{
				Result = false;
				JID = null;
			}

			IsFriendEventArgs e2 = new IsFriendEventArgs(e, State, JID, Result);
			try
			{
				Callback(this, e2);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private void UnfriendHandler(object Sender, MessageEventArgs e)
		{
			if (e.From == this.provisioningServerAddress)
			{
				string Jid = XML.Attribute(e.Content, "jid");

				if (!string.IsNullOrEmpty(Jid))
					this.client.RequestPresenceUnsubscription(Jid);
			}
		}

		private void FriendHandler(object Sender, MessageEventArgs e)
		{
			if (e.From == this.provisioningServerAddress)
			{
				string Jid = XML.Attribute(e.Content, "jid");

				if (!string.IsNullOrEmpty(Jid))
				{
					this.IsFriend(Jid, (sender, e2) =>
					{
						if (e2.Ok && e2.Friend)
							this.client.RequestPresenceSubscription(Jid);

					}, null);
				}
			}
		}

	}
}
