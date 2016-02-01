using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Defined in the internet draft "A New HTTP Status Code for Legally-restricted Resources". Intended to be used when resource access is 
	/// denied for legal reasons, e.g. censorship or government-mandated blocked access.
	/// </summary>
	public class UnavailableForLegalReasonsException : HttpException
	{
		/// <summary>
		/// Defined in the internet draft "A New HTTP Status Code for Legally-restricted Resources". Intended to be used when resource access is 
		/// denied for legal reasons, e.g. censorship or government-mandated blocked access.
		/// </summary>
		public UnavailableForLegalReasonsException()
			: base(451, "Unavailable For Legal Reasons")
		{
		}
	}
}
