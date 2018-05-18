using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Identifier is not supported, but may be in future
	/// </summary>
	public class AcmeUnsupportedIdentifierException : AcmeException
	{
		internal AcmeUnsupportedIdentifierException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Type, Detail, Status, Subproblems)
		{
		}
	}
}
