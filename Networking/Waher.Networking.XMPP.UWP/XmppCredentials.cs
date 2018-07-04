using System;
using System.Collections.Generic;
using System.Text;
#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Certificates;
#else
using System.Security.Cryptography.X509Certificates;
#endif

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Class containing credentials for an XMPP client connection.
	/// </summary>
	public class XmppCredentials
	{
		/// <summary>
		/// Default XMPP Server port.
		/// </summary>
		public const int DefaultPort = 5222;

#if WINDOWS_UWP
		private Certificate clientCertificate = null;
#else
		private X509Certificate clientCertificate = null;
#endif
		private string host = string.Empty;
		private string account = string.Empty;
		private string password = string.Empty;
		private string passwordType = string.Empty;
		private string thingRegistry = string.Empty;
		private string provisioning = string.Empty;
		private string events = string.Empty;
		private string formSignatureKey = string.Empty;
		private string formSignatureSecret = string.Empty;
		private string httpEndpoint = string.Empty;
		private bool sniffer = false;
		private bool trustServer = false;
		private bool allowCramMD5 = true;
		private bool allowDigestMD5 = true;
		private bool allowPlain = false;
		private bool allowScramSHA1 = true;
		private bool allowEncryption = true;
		private bool allowRegistration = false;
		private bool requestRosterOnStartup = true;
		private int port = DefaultPort;

		/// <summary>
		/// Class containing credentials for an XMPP client connection.
		/// </summary>
		public XmppCredentials()
		{
		}

		/// <summary>
		/// Host name of XMPP server.
		/// </summary>
		public string Host
		{
			get { return this.host; }
			set { this.host = value; }
		}

		/// <summary>
		/// Name of account on XMPP server to connect to.
		/// </summary>
		public string Account
		{
			get { return this.account; }
			set { this.account = value; }
		}

		/// <summary>
		/// Password of account.
		/// </summary>
		public string Password
		{
			get { return this.password; }
			set { this.password = value; }
		}

		/// <summary>
		/// Password type of account (empty string = normal password, otherwise, HASH method used in authentication mechanism).
		/// </summary>
		public string PasswordType
		{
			get { return this.passwordType; }
			set { this.passwordType = value; }
		}

		/// <summary>
		/// JID of Thing Registry to use. Leave blank if no thing registry is to be used.
		/// </summary>
		public string ThingRegistry
		{
			get { return this.thingRegistry; }
			set { this.thingRegistry = value; }
		}

		/// <summary>
		/// JID of Provisioning Server to use. Leave blank if no thing registry is to be used.
		/// </summary>
		public string Provisioning
		{
			get { return this.provisioning; }
			set { this.provisioning = value; }
		}

		/// <summary>
		/// JID of entity to whom events should be sent. Leave blank if events are not to be forwarded.
		/// </summary>
		public string Events
		{
			get { return this.events; }
			set { this.events = value; }
		}

		/// <summary>
		/// If a sniffer is to be used ('true' or 'false'). If 'true', network communication will be output to the console.
		/// </summary>
		public bool Sniffer
		{
			get { return this.sniffer; }
			set { this.sniffer = value; }
		}

		/// <summary>
		/// Port number to use when connecting to XMPP server.
		/// </summary>
		public int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		/// <summary>
		/// If the server certificate should be trusted automatically ('true'), or if a certificate validation should be done to 
		/// test the validity of the server ('false').
		/// </summary>
		public bool TrustServer
		{
			get { return this.trustServer; }
			set { this.trustServer = value; }
		}

		/// <summary>
		/// If CRAM-MD5 should be allowed, during authentication.
		/// </summary>
		public bool AllowCramMD5
		{
			get { return this.allowCramMD5; }
			set { this.allowCramMD5 = value; }
		}

		/// <summary>
		/// If DIGEST-MD5 should be allowed, during authentication.
		/// </summary>
		public bool AllowDigestMD5
		{
			get { return this.allowDigestMD5; }
			set { this.allowDigestMD5 = value; }
		}

		/// <summary>
		/// If PLAIN should be allowed, during authentication.
		/// </summary>
		public bool AllowPlain
		{
			get { return this.allowPlain; }
			set { this.allowPlain = value; }
		}

		/// <summary>
		/// If SCRAM-SHA-1 should be allowed, during authentication.
		/// </summary>
		public bool AllowScramSHA1
		{
			get { return this.allowScramSHA1; }
			set { this.allowScramSHA1 = value; }
		}

		/// <summary>
		/// If encryption should be allowed or not.
		/// </summary>
		public bool AllowEncryption
		{
			get { return this.allowEncryption; }
			set { this.allowEncryption = value; }
		}

		/// <summary>
		/// If the roster should be requested during startup.
		/// </summary>
		public bool RequestRosterOnStartup
		{
			get { return this.requestRosterOnStartup; }
			set { this.requestRosterOnStartup = value; }
		}

		/// <summary>
		/// If the client is allowed to register for a new account, if the account was not found.
		/// </summary>
		public bool AllowRegistration
		{
			get { return this.allowRegistration; }
			set { this.allowRegistration = value; }
		}

		/// <summary>
		/// Form signature key, if form signatures (XEP-0348) is to be used during registration.
		/// </summary>
		public string FormSignatureKey
		{
			get { return this.formSignatureKey; }
			set { this.formSignatureKey = value; }
		}

		/// <summary>
		/// Form signature secret, if form signatures (XEP-0348) is to be used during registration.
		/// </summary>
		public string FormSignatureSecret
		{
			get { return this.formSignatureSecret; }
			set { this.formSignatureSecret = value; }
		}

		/// <summary>
		/// Alternative HTTP binding (BOSH) if a traditional binary socket connection is not possible or desirable to use.
		/// </summary>
		public string HttpEndpoint
		{
			get { return this.httpEndpoint; }
			set { this.httpEndpoint = value; }
		}

#if WINDOWS_UWP
		/// <summary>
		/// Client certificate.
		/// </summary>
		public Certificate ClientCertificate
#else
		/// <summary>
		/// Client certificate.
		/// </summary>
		public X509Certificate ClientCertificate
#endif
		{
			get { return this.clientCertificate; }
			set { this.clientCertificate = value; }
		}

	}
}
