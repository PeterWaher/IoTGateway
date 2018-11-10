using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server is unable to store the representation needed to complete the request.
	/// </summary>
	public class InsufficientStorageException : HttpException
	{
		private const int Code = 507;
		private const string Msg = "Insufficient Storage";

		/// <summary>
		/// The server is unable to store the representation needed to complete the request.
		/// </summary>
		public InsufficientStorageException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The server is unable to store the representation needed to complete the request.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public InsufficientStorageException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The server is unable to store the representation needed to complete the request.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public InsufficientStorageException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
