using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Exceptions
{
	/// <summary>
	/// Base class from ASN.1 exceptions.
	/// </summary>
	public class Asn1Exception : Exception
	{
		/// <summary>
		/// Base class from ASN.1 exceptions.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		public Asn1Exception(string Message)
			: base(Message)
		{
		}
	}
}
