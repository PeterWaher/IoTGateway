using System;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server does not support the functionality required to fulfill the request. This is the appropriate response when the server does not 
	/// recognize the request method and is not capable of supporting it for any resource. 
	/// </summary>
	public class NotImplementedException : HttpException
	{
		/// <summary>
		/// 501
		/// </summary>
		public const int Code = 501;

		/// <summary>
		/// Not Implemented
		/// </summary>
		public const string StatusMessage = "Not Implemented";

		/// <summary>
		/// The server does not support the functionality required to fulfill the request. This is the appropriate response when the server does not 
		/// recognize the request method and is not capable of supporting it for any resource. 
		/// </summary>
		public NotImplementedException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// The server does not support the functionality required to fulfill the request. This is the appropriate response when the server does not 
		/// recognize the request method and is not capable of supporting it for any resource. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public NotImplementedException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// The server does not support the functionality required to fulfill the request. This is the appropriate response when the server does not 
		/// recognize the request method and is not capable of supporting it for any resource. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public NotImplementedException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
