using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The request could not be understood by the server due to malformed syntax. The client SHOULD NOT repeat the request without modifications. 
	/// </summary>
	public class BadRequestException : HttpException
	{
		/// <summary>
		/// 400
		/// </summary>
		public const int Code = 400;

		/// <summary>
		/// Bad Request
		/// </summary>
		public const string StatusMessage = "Bad Request";

		/// <summary>
		/// The request could not be understood by the server due to malformed syntax. The client SHOULD NOT repeat the request without modifications. 
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public BadRequestException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The request could not be understood by the server due to malformed syntax. The client SHOULD NOT repeat the request without modifications. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public BadRequestException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The request could not be understood by the server due to malformed syntax. The client SHOULD NOT repeat the request without modifications. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public BadRequestException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
