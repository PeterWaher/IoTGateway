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
		private readonly Uri accountLocation;
		private Uri location = null;

		/// <summary>
		/// Abstract base class for all ACME objects.
		/// </summary>
		/// <param name="Client">ACME client.</param>
		/// <param name="AccountLocation">Account resource location.</param>
		/// <param name="Location">ACME resource location.</param>
		public AcmeResource(AcmeClient Client, Uri AccountLocation, Uri Location)
			: base(Client)
		{
			this.accountLocation = AccountLocation;
			this.location = Location;
		}

		/// <summary>
		/// Account location.
		/// </summary>
		public Uri AccountLocation => this.accountLocation;

		/// <summary>
		/// Location of resource.
		/// </summary>
		public Uri Location
		{
			get { return this.location; }
			protected set { this.location = value; }
		}
	}
}
