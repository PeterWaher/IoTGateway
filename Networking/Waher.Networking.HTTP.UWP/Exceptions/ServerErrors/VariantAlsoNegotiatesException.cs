using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Transparent content negotiation for the request results in a circular reference.
	/// </summary>
	public class VariantAlsoNegotiatesException : HttpException
	{
		/// <summary>
		/// Transparent content negotiation for the request results in a circular reference.
		/// </summary>
		public VariantAlsoNegotiatesException()
			: base(506, "Variant Also Negotiates")
		{
		}
	}
}
