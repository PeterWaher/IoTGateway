using System;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
	/// </summary>
	public class NotFoundException : HttpException
	{
		/// <summary>
		/// 404
		/// </summary>
		public const int Code = 404;

		/// <summary>
		/// Not Found
		/// </summary>
		public const string StatusMessage = "Not Found";

		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		public NotFoundException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public NotFoundException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public NotFoundException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
