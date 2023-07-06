using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Further extensions to the request are required for the server to fulfil it.
	/// </summary>
	public class NotExtendedException : HttpException
	{
		/// <summary>
		/// 510
		/// </summary>
		public const int Code = 510;

		/// <summary>
		/// Not Extended
		/// </summary>
		public const string StatusMessage = "Not Extended";

		/// <summary>
		/// Further extensions to the request are required for the server to fulfil it.
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public NotExtendedException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// Further extensions to the request are required for the server to fulfil it.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public NotExtendedException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// Further extensions to the request are required for the server to fulfil it.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public NotExtendedException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
