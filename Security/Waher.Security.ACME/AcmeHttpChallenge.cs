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
	}
}
