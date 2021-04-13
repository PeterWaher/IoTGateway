using System;
#if WINDOWS_UWP
using Windows.Security.Cryptography.Certificates;
#else
using System.Security.Cryptography.X509Certificates;
#endif

namespace Waher.Networking.XMPP.Provisioning
{
	internal class CertificateUse
	{
		private readonly string token;
#if WINDOWS_UWP
		private readonly Certificate localCertificate;
#else
		private readonly X509Certificate2 localCertificate;
#endif
		private string remoteCertificateJid;
		private DateTime lastUse = DateTime.Now;

#if WINDOWS_UWP
		public CertificateUse(string Token, Certificate LocalCertificate)
#else
		public CertificateUse(string Token, X509Certificate2 LocalCertificate)
#endif
		{
			this.token = Token;
			this.localCertificate = LocalCertificate;
			this.remoteCertificateJid = null;
		}

		public CertificateUse(string Token, string RemoteCertificateJid)
		{
			this.token = Token;
			this.localCertificate = null;
			this.remoteCertificateJid = RemoteCertificateJid;
		}

		/// <summary>
		/// Token
		/// </summary>
		public string Token
		{
			get { return this.token; }
		}

		/// <summary>
		/// Local certificate, or null if token received from another entity.
		/// </summary>
#if WINDOWS_UWP
		public Certificate LocalCertificate
#else
		public X509Certificate2 LocalCertificate
#endif
		{
			get { return this.localCertificate; }
		}

		/// <summary>
		/// JID of remote entity sending the token, if certificate is used on another device.
		/// </summary>
		public string RemoteCertificateJid
		{
			get { return this.remoteCertificateJid; }
			internal set { this.remoteCertificateJid = value; }
		}

		/// <summary>
		/// Last use of certificate.
		/// </summary>
		public DateTime LastUse
		{
			get { return this.lastUse; }
			internal set { this.lastUse = value; }
		}
	}
}
