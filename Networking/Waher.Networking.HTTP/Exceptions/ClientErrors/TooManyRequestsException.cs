using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The user has sent too many requests in a given amount of time. Intended for use with rate limiting schemes.
	/// </summary>
	public class TooManyRequestsException : HttpException
	{
		/// <summary>
		/// 429
		/// </summary>
		public const int Code = 429;

		/// <summary>
		/// Too Many Requests
		/// </summary>
		public const string StatusMessage = "Too Many Requests";

		/// <summary>
		/// The user has sent too many requests in a given amount of time. Intended for use with rate limiting schemes.
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public TooManyRequestsException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The user has sent too many requests in a given amount of time. Intended for use with rate limiting schemes.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public TooManyRequestsException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The user has sent too many requests in a given amount of time. Intended for use with rate limiting schemes.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public TooManyRequestsException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
