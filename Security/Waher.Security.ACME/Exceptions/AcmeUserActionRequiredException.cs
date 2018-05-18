using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Visit the "instance" URL and take actions specified there
	/// </summary>
	public class AcmeUserActionRequiredException : AcmeException
	{
		internal AcmeUserActionRequiredException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Type, Detail, Status, Subproblems)
		{
		}
	}
}
