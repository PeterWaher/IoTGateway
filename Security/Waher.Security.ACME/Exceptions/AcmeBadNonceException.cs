using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// The client sent an unacceptable anti-replay nonce
	/// </summary>
	public class AcmeBadNonceException : AcmeException
	{
		internal AcmeBadNonceException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Type, Detail, Status, Subproblems)
		{
		}
	}
}
