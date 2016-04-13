using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Waher.Networking.XMPP.Provisioning
{
	internal class CertificateUse
	{
		private string token;
		private X509Certificate2 localCertificate;
		private string remoteCertificateJid;
		private DateTime lastUse = DateTime.Now;

		public CertificateUse(string Token, X509Certificate2 LocalCertificate)
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
		public X509Certificate2 LocalCertificate
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
