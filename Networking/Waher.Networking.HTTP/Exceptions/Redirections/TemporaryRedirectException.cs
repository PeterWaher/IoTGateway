using System.Collections.Generic;
using Waher.Events;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The requested resource resides temporarily under a different URI. Since the redirection MAY be altered on occasion, the client SHOULD continue 
	/// to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
	/// </summary>
	public class TemporaryRedirectException : HttpException
	{
		/// <summary>
		/// 307
		/// </summary>
		public const int Code = 307;

		/// <summary>
		/// Temporary Redirect
		/// </summary>
		public const string StatusMessage = "Temporary Redirect";

		/// <summary>
		/// The requested resource resides temporarily under a different URI. Since the redirection MAY be altered on occasion, the client SHOULD continue 
		/// to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public TemporaryRedirectException(string Location, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields.Join(new KeyValuePair<string, string>("Location", Location)))
		{
		}

		/// <summary>
		/// The requested resource resides temporarily under a different URI. Since the redirection MAY be altered on occasion, the client SHOULD continue 
		/// to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public TemporaryRedirectException(string Location, object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields.Join(new KeyValuePair<string, string>("Location", Location)))
		{
		}

		/// <summary>
		/// The requested resource resides temporarily under a different URI. Since the redirection MAY be altered on occasion, the client SHOULD continue 
		/// to use the Request-URI for future requests. This response is only cacheable if indicated by a Cache-Control or Expires header field. 
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public TemporaryRedirectException(string Location, byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields.Join(new KeyValuePair<string, string>("Location", Location)))
		{
		}
	}
}
