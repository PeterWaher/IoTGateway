using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Abstract base class for all ACME resources.
	/// </summary>
    public abstract class AcmeResource : AcmeObject
    {
		private readonly Uri location = null;

		/// <summary>
		/// Abstract base class for all ACME objects.
		/// </summary>
		/// <param name="Client">ACME client.</param>
		/// <param name="Location">ACME resource location.</param>
		public AcmeResource(AcmeClient Client, Uri Location)
			: base(Client)
		{
			this.location = Location;
		}

		/// <summary>
		/// Location of resource.
		/// </summary>
		public Uri Location => this.location;
	}
}
