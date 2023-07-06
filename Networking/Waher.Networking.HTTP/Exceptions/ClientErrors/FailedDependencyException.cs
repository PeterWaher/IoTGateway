using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The request failed due to failure of a previous request (e.g., a PROPPATCH).
	/// </summary>
	public class FailedDependencyException : HttpException
	{
		/// <summary>
		/// 424
		/// </summary>
		public const int Code = 424;

		/// <summary>
		/// Failed Dependency
		/// </summary>
		public const string StatusMessage = "Failed Dependency";

		/// <summary>
		/// The request failed due to failure of a previous request (e.g., a PROPPATCH).
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public FailedDependencyException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The request failed due to failure of a previous request (e.g., a PROPPATCH).
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public FailedDependencyException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The request failed due to failure of a previous request (e.g., a PROPPATCH).
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public FailedDependencyException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
