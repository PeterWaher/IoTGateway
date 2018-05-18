using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// A contact URL for an account used an unsupported protocol scheme
	/// </summary>
	public class AcmeUnsupportedContactException : AcmeException
	{
		internal AcmeUnsupportedContactException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Type, Detail, Status, Subproblems)
		{
		}
	}
}
