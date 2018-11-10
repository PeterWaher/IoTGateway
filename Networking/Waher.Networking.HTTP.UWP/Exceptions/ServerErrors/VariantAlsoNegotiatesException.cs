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
		private const int Code = 506;
		private const string Msg = "Variant Also Negotiates";

		/// <summary>
		/// Transparent content negotiation for the request results in a circular reference.
		/// </summary>
		public VariantAlsoNegotiatesException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// Transparent content negotiation for the request results in a circular reference.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public VariantAlsoNegotiatesException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// Transparent content negotiation for the request results in a circular reference.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public VariantAlsoNegotiatesException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
