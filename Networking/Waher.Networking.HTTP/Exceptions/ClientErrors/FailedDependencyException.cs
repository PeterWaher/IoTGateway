using System;

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
		public FailedDependencyException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// The request failed due to failure of a previous request (e.g., a PROPPATCH).
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public FailedDependencyException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// The request failed due to failure of a previous request (e.g., a PROPPATCH).
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public FailedDependencyException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
