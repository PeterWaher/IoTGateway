using System.Collections.Generic;
using Waher.Events;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The requested resource has been assigned a new permanent URI and any future references to this resource SHOULD use one of the returned URIs. 
	/// Clients with link editing capabilities ought to automatically re-link references to the Request-URI to one or more of the new references 
	/// returned by the server, where possible. This response is cacheable unless indicated otherwise. 
	/// </summary>
	public class MovedPermanentlyException : HttpException
	{
		/// <summary>
		/// 301
		/// </summary>
		public const int Code = 301;

		/// <summary>
		/// Moved Permanently
		/// </summary>
		public const string StatusMessage = "Moved Permanently";

		/// <summary>
		/// The requested resource has been assigned a new permanent URI and any future references to this resource SHOULD use one of the returned URIs. 
		/// Clients with link editing capabilities ought to automatically re-link references to the Request-URI to one or more of the new references 
		/// returned by the server, where possible. This response is cacheable unless indicated otherwise. 
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public MovedPermanentlyException(string Location, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields.Join(new KeyValuePair<string, string>("Location", Location)))
		{
		}

		/// <summary>
		/// The requested resource has been assigned a new permanent URI and any future references to this resource SHOULD use one of the returned URIs. 
		/// Clients with link editing capabilities ought to automatically re-link references to the Request-URI to one or more of the new references 
		/// returned by the server, where possible. This response is cacheable unless indicated otherwise. 
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public MovedPermanentlyException(string Location, object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields.Join(new KeyValuePair<string, string>("Location", Location)))
		{
		}

		/// <summary>
		/// The requested resource has been assigned a new permanent URI and any future references to this resource SHOULD use one of the returned URIs. 
		/// Clients with link editing capabilities ought to automatically re-link references to the Request-URI to one or more of the new references 
		/// returned by the server, where possible. This response is cacheable unless indicated otherwise. 
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public MovedPermanentlyException(string Location, byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields.Join(new KeyValuePair<string, string>("Location", Location)))
		{
		}
	}
}
