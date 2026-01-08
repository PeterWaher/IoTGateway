using System.Security.Cryptography.X509Certificates;

namespace Waher.Security
{
	/// <summary>
	/// Interface for Mutual TLS (mTLS) Clients or TLS servers.
	/// </summary>
	public interface ITlsCertificateEndpoint
	{
		/// <summary>
		/// Updates the certificate used in mTLS negotiation.
		/// </summary>
		/// <param name="Certificate">Updated Certificate</param>
		void UpdateCertificate(X509Certificate Certificate);
	}
}
