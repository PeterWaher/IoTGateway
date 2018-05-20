using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Represents an ACME HTTP challenge.
	/// </summary>
	public class AcmeHttpChallenge : AcmeChallenge
	{
		internal AcmeHttpChallenge(AcmeClient Client, Uri AccountLocation, IEnumerable<KeyValuePair<string, object>> Obj)
			: base(Client, AccountLocation, Obj)
		{
		}

		/// <summary>
		/// Challenge resource. The HTTP server should return <see cref="AcmeChallenge.KeyAuthorization"/>
		/// on this resource, using Content-Type application/octet-stream.
		/// </summary>
		public string ResourceName
		{
			get { return "/.well-known/acme-challenge/" + this.Token; }
		}
	}
}
