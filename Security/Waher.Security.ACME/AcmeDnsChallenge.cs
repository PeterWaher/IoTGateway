using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Represents an ACME DNS challenge.
	/// </summary>
	public class AcmeDnsChallenge : AcmeChallenge
	{
		internal AcmeDnsChallenge(AcmeClient Client, Uri AccountLocation, IEnumerable<KeyValuePair<string, object>> Obj)
			: base(Client, AccountLocation, Obj)
		{
		}

		/// <summary>
		/// Domain name prefix used to validate DNS domain name ownership.
		/// </summary>
		public string ValidationDomainNamePrefix
		{
			get { return "_acme-challenge."; }
		}

		/// <summary>
		/// Key authorization string. Used as response to challenge.
		/// Should be added to a TXT record to the domain name prefixed by
		/// <see cref="ValidationDomainNamePrefix"/>.
		/// </summary>
		public override string KeyAuthorization
		{
			get
			{
				return Base64Url.Encode(Hashes.ComputeSHA256Hash(Encoding.ASCII.GetBytes(base.KeyAuthorization)));
			}
		}

	}
}
