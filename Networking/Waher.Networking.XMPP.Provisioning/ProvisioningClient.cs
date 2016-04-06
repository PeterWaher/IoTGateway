using System;
using System.Collections.Generic;
using System.Xml;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Waher.Content;
using Waher.Events;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Provisioning
{
	public delegate void TokenCallback(object Sender, TokenEventArgs e);

	/// <summary>
	/// Implements an XMPP provisioning client interface.
	/// 
	/// The interface is defined in XEP-0324:
	/// http://xmpp.org/extensions/xep-0324.html
	/// </summary>
	public class ProvisioningClient : IDisposable
	{
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
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
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

			if (E != null && E.LocalName == "getTokenChallenge" && E.NamespaceURI == NamespaceProvisioning)
			{
				int SeqNr = XML.Attribute(E, "seqnr", 0);
				string Challenge = E.InnerText;
				byte[] Bin = System.Convert.FromBase64String(Challenge);
				Bin = ((RSACryptoServiceProvider)Certificate.PrivateKey).Decrypt(Bin, false);
				string Response = System.Convert.ToBase64String(Bin, Base64FormattingOptions.None);

				this.client.SendIqGet(this.provisioningServerAddress, "<getTokenChallengeResponse xmlns='urn:xmpp:iot:provisioning' seqnr='" +
					SeqNr.ToString()+"'>" + Response + "</getTokenChallengeResponse>",
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

			if (E != null && E.LocalName == "getTokenResponse" && E.NamespaceURI == NamespaceProvisioning)
				Token = XML.Attribute(E, "token");
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

	}
}
