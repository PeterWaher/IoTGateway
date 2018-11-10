using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The user has sent too many requests in a given amount of time. Intended for use with rate limiting schemes.
	/// </summary>
	public class TooManyRequestsException : HttpException
	{
		private const int Code = 429;
		private const string Msg = "Too Many Requests";

		/// <summary>
		/// The user has sent too many requests in a given amount of time. Intended for use with rate limiting schemes.
		/// </summary>
		public TooManyRequestsException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The user has sent too many requests in a given amount of time. Intended for use with rate limiting schemes.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public TooManyRequestsException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The user has sent too many requests in a given amount of time. Intended for use with rate limiting schemes.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public TooManyRequestsException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
