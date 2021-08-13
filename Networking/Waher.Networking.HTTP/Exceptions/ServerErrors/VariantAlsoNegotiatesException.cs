using System;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Transparent content negotiation for the request results in a circular reference.
	/// </summary>
	public class VariantAlsoNegotiatesException : HttpException
	{
		/// <summary>
		/// 506
		/// </summary>
		public const int Code = 506;

		/// <summary>
		/// Variant Also Negotiates
		/// </summary>
		public const string StatusMessage = "Variant Also Negotiates";

		/// <summary>
		/// Transparent content negotiation for the request results in a circular reference.
		/// </summary>
		public VariantAlsoNegotiatesException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// Transparent content negotiation for the request results in a circular reference.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public VariantAlsoNegotiatesException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// Transparent content negotiation for the request results in a circular reference.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public VariantAlsoNegotiatesException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
