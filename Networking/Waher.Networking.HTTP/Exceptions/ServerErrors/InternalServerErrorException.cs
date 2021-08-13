using System;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
	/// </summary>
	public class InternalServerErrorException : HttpException
	{
		/// <summary>
		/// 500
		/// </summary>
		public const int Code = 500;

		/// <summary>
		/// Internal Server Error
		/// </summary>
		public const string StatusMessage = "Internal Server Error";

		/// <summary>
		/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
		/// </summary>
		public InternalServerErrorException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public InternalServerErrorException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public InternalServerErrorException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
