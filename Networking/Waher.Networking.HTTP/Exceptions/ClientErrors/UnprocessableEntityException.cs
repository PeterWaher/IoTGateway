using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The request was well-formed but was unable to be followed due to semantic errors.
	/// </summary>
	public class UnprocessableEntityException : HttpException
	{
		/// <summary>
		/// 422
		/// </summary>
		public const int Code = 422;

		/// <summary>
		/// Unprocessable Entity
		/// </summary>
		public const string StatusMessage = "Unprocessable Entity";

		/// <summary>
		/// The request was well-formed but was unable to be followed due to semantic errors.
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public UnprocessableEntityException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The request was well-formed but was unable to be followed due to semantic errors.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public UnprocessableEntityException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The request was well-formed but was unable to be followed due to semantic errors.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public UnprocessableEntityException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
