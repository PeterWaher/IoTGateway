using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The requested resource resides temporarily under a different URI. Since the redirection might be altered on occasion, the client SHOULD 
	/// continue to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
	/// </summary>
	public class FoundException : HttpException
	{
		/// <summary>
		/// 302
		/// </summary>
		public const int Code = 302;

		/// <summary>
		/// Found
		/// </summary>
		public const string StatusMessage = "Found";

		/// <summary>
		/// The requested resource resides temporarily under a different URI. Since the redirection might be altered on occasion, the client SHOULD 
		/// continue to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
		/// </summary>
		/// <param name="Location">Location.</param>
		public FoundException(string Location)
			: base(Code, StatusMessage, new KeyValuePair<string, string>("Location", Location))
		{
		}

		/// <summary>
		/// The requested resource resides temporarily under a different URI. Since the redirection might be altered on occasion, the client SHOULD 
		/// continue to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public FoundException(string Location, object ContentObject)
			: base(Code, StatusMessage, ContentObject, new KeyValuePair<string, string>("Location", Location))
		{
		}

		/// <summary>
		/// The requested resource resides temporarily under a different URI. Since the redirection might be altered on occasion, the client SHOULD 
		/// continue to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public FoundException(string Location, byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType, new KeyValuePair<string, string>("Location", Location))
		{
		}
	}
}
