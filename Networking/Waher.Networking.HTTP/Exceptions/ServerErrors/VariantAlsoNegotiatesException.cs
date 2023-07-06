using System.Collections.Generic;

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
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public VariantAlsoNegotiatesException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// Transparent content negotiation for the request results in a circular reference.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public VariantAlsoNegotiatesException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// Transparent content negotiation for the request results in a circular reference.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public VariantAlsoNegotiatesException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
