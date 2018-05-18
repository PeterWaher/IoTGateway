using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Certification Authority Authorization (CAA) records forbid the CA from issuing
	/// </summary>
	public class AcmeCaaException : AcmeException
	{
		internal AcmeCaaException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Type, Detail, Status, Subproblems)
		{
		}
	}
}
