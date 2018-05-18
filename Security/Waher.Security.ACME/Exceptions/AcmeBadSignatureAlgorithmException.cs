using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// The JWS was signed with an algorithm the server does not support
	/// </summary>
	public class AcmeBadSignatureAlgorithmException : AcmeException
	{
		internal AcmeBadSignatureAlgorithmException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Type, Detail, Status, Subproblems)
		{
		}
	}
}
