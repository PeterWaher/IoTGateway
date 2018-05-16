using System;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Implements an ACME client for the generation of certificates using ACME-compliant certificate servers.
	/// </summary>
    public class AcmeClient
    {
		private readonly string httpEndpoint;

		/// <summary>
		/// Implements an ACME client for the generation of certificates using ACME-compliant certificate servers.
		/// </summary>
		/// <param name="HttpEndpoint">HTTP endpoint for the ACME server.</param>
		public AcmeClient(string HttpEndpoint)
		{
			this.httpEndpoint = HttpEndpoint;
		}
    }
}
