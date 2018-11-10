using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The request could not be understood by the server due to malformed syntax. The client SHOULD NOT repeat the request without modifications. 
	/// </summary>
	public class BadRequestException : HttpException
	{
		private const int Code = 400;
		private const string Msg = "Bad Request";

		/// <summary>
		/// The request could not be understood by the server due to malformed syntax. The client SHOULD NOT repeat the request without modifications. 
		/// </summary>
		public BadRequestException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The request could not be understood by the server due to malformed syntax. The client SHOULD NOT repeat the request without modifications. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public BadRequestException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The request could not be understood by the server due to malformed syntax. The client SHOULD NOT repeat the request without modifications. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public BadRequestException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
