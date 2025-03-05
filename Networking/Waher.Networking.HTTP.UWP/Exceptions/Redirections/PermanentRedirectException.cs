using System.Collections.Generic;
using Waher.Events;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// This means that the resource is now permanently located at another URI, specified by the 
	/// Location: HTTP Response header. This has the same semantics as the 301 Moved Permanently 
	/// HTTP response code, with the exception that the user agent must not change the HTTP method 
	/// used: if a POST was used in the first request, a POST must be used in the second request.
	/// </summary>
	public class PermanentRedirectException : HttpException
	{
		/// <summary>
		/// 307
		/// </summary>
		public const int Code = 308;

		/// <summary>
		/// Temporary Redirect
		/// </summary>
		public const string StatusMessage = "Permanent Redirect";

		/// <summary>
		/// This means that the resource is now permanently located at another URI, specified by the 
		/// Location: HTTP Response header. This has the same semantics as the 301 Moved Permanently 
		/// HTTP response code, with the exception that the user agent must not change the HTTP method 
		/// used: if a POST was used in the first request, a POST must be used in the second request.
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public PermanentRedirectException(string Location, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields.Join(new KeyValuePair<string, string>("Location", Location)))
		{
		}

		/// <summary>
		/// This means that the resource is now permanently located at another URI, specified by the 
		/// Location: HTTP Response header. This has the same semantics as the 301 Moved Permanently 
		/// HTTP response code, with the exception that the user agent must not change the HTTP method 
		/// used: if a POST was used in the first request, a POST must be used in the second request.
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public PermanentRedirectException(string Location, object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields.Join(new KeyValuePair<string, string>("Location", Location)))
		{
		}

		/// <summary>
		/// This means that the resource is now permanently located at another URI, specified by the 
		/// Location: HTTP Response header. This has the same semantics as the 301 Moved Permanently 
		/// HTTP response code, with the exception that the user agent must not change the HTTP method 
		/// used: if a POST was used in the first request, a POST must be used in the second request.
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public PermanentRedirectException(string Location, byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields.Join(new KeyValuePair<string, string>("Location", Location)))
		{
		}
	}
}
