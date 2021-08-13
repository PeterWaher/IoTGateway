using System;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server is unable to store the representation needed to complete the request.
	/// </summary>
	public class InsufficientStorageException : HttpException
	{
		/// <summary>
		/// 507
		/// </summary>
		public const int Code = 507;

		/// <summary>
		/// Insufficient Storage
		/// </summary>
		public const string StatusMessage = "Insufficient Storage";

		/// <summary>
		/// The server is unable to store the representation needed to complete the request.
		/// </summary>
		public InsufficientStorageException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// The server is unable to store the representation needed to complete the request.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public InsufficientStorageException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// The server is unable to store the representation needed to complete the request.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public InsufficientStorageException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
