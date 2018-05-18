using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// The CSR is unacceptable (e.g., due to a short key)
	/// </summary>
	public class AcmeBadCsrException : AcmeException
	{
		internal AcmeBadCsrException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Type, Detail, Status, Subproblems)
		{
		}
	}
}
