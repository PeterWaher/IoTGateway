using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// The request message was malformed
	/// </summary>
	public class AcmeMalformedException : AcmeException
	{
		internal AcmeMalformedException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Type, Detail, Status, Subproblems)
		{
		}
	}
}
