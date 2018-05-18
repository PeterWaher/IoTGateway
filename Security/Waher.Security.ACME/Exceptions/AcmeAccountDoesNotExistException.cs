using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Security.ACME
{
	/// <summary>
	/// The request specified an account that does not exist
	/// </summary>
	public class AcmeAccountDoesNotExistException : AcmeException
	{
		internal AcmeAccountDoesNotExistException(string Type, string Detail, int? Status, AcmeException[] Subproblems)
			: base(Type, Detail, Status, Subproblems)
		{
		}
	}
}
