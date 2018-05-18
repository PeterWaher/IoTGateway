using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// The request must include a value for the "externalAccountBinding" field
	/// </summary>
	public class AcmeExternalAccountRequiredException : AcmeException
	{
		internal AcmeExternalAccountRequiredException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Type, Detail, Status, Subproblems)
		{
		}
	}
}
