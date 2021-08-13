using System;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The resource identified by the request is only capable of generating response entities which have content characteristics not acceptable 
	/// according to the accept headers sent in the request. 
	/// </summary>
	public class NotAcceptableException : HttpException
	{
		/// <summary>
		/// 406
		/// </summary>
		public const int Code = 406;

		/// <summary>
		/// Not Acceptable
		/// </summary>
		public const string StatusMessage = "Not Acceptable";

		/// <summary>
		/// The resource identified by the request is only capable of generating response entities which have content characteristics not acceptable 
		/// according to the accept headers sent in the request. 
		/// </summary>
		public NotAcceptableException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// The resource identified by the request is only capable of generating response entities which have content characteristics not acceptable 
		/// according to the accept headers sent in the request. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public NotAcceptableException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// The resource identified by the request is only capable of generating response entities which have content characteristics not acceptable 
		/// according to the accept headers sent in the request. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public NotAcceptableException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
