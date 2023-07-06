using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server detected an infinite loop while processing the request.
	/// </summary>
	public class LoopDetectedException : HttpException
	{
		/// <summary>
		/// 508
		/// </summary>
		public const int Code = 508;

		/// <summary>
		/// Loop Detected
		/// </summary>
		public const string StatusMessage = "Loop Detected";

		/// <summary>
		/// The server detected an infinite loop while processing the request.
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public LoopDetectedException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The server detected an infinite loop while processing the request.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public LoopDetectedException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The server detected an infinite loop while processing the request.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public LoopDetectedException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
