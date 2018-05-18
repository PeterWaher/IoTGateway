using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Abstract base class for all ACME objects.
	/// </summary>
    public abstract class AcmeObject
    {
		private readonly AcmeClient client;

		/// <summary>
		/// Abstract base class for all ACME objects.
		/// </summary>
		/// <param name="Client">ACME client.</param>
		public AcmeObject(AcmeClient Client)
		{
			this.client = Client;
		}

		/// <summary>
		/// ACME client.
		/// </summary>
		public AcmeClient Client => this.client;
    }
}
