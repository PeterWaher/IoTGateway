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
	}
}
