using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// The revocation reason provided is not allowed by the server
	/// </summary>
	public class AcmeBadRevocationReasonException : AcmeException
	{
		internal AcmeBadRevocationReasonException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Type, Detail, Status, Subproblems)
		{
		}
	}
}
