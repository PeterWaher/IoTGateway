using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The requested resource has been assigned a new permanent URI and any future references to this resource SHOULD use one of the returned URIs. 
	/// Clients with link editing capabilities ought to automatically re-link references to the Request-URI to one or more of the new references 
	/// returned by the server, where possible. This response is cacheable unless indicated otherwise. 
	/// </summary>
	public class MovedPermanentlyException : HttpException
	{
		private const int Code = 301;
		private const string Msg = "Moved Permanently";

		/// <summary>
		/// The requested resource has been assigned a new permanent URI and any future references to this resource SHOULD use one of the returned URIs. 
		/// Clients with link editing capabilities ought to automatically re-link references to the Request-URI to one or more of the new references 
		/// returned by the server, where possible. This response is cacheable unless indicated otherwise. 
		/// </summary>
		/// <param name="Location">Location.</param>
		public MovedPermanentlyException(string Location)
			: base(Code, Msg, new KeyValuePair<string, string>("Location", Location))
		{
		}

		/// <summary>
		/// The requested resource has been assigned a new permanent URI and any future references to this resource SHOULD use one of the returned URIs. 
		/// Clients with link editing capabilities ought to automatically re-link references to the Request-URI to one or more of the new references 
		/// returned by the server, where possible. This response is cacheable unless indicated otherwise. 
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public MovedPermanentlyException(string Location, object ContentObject)
			: base(Code, Msg, ContentObject, new KeyValuePair<string, string>("Location", Location))
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
		public MovedPermanentlyException(string Location, byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType, new KeyValuePair<string, string>("Location", Location))
		{
		}
	}
}
