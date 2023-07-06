using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The requested resource is no longer available at the server and no forwarding address is known. This condition is expected to be considered 
	/// permanent. Clients with link editing capabilities SHOULD delete references to the Request-URI after user approval. If the server does not 
	/// know, or has no facility to determine, whether or not the condition is permanent, the status code 404 (Not Found) SHOULD be used instead. 
	/// This response is cacheable unless indicated otherwise.
	/// </summary>
	public class GoneException : HttpException
	{
		/// <summary>
		/// 410
		/// </summary>
		public const int Code = 410;

		/// <summary>
		/// Gone
		/// </summary>
		public const string StatusMessage = "Gone";

		/// <summary>
		/// The requested resource is no longer available at the server and no forwarding address is known. This condition is expected to be considered 
		/// permanent. Clients with link editing capabilities SHOULD delete references to the Request-URI after user approval. If the server does not 
		/// know, or has no facility to determine, whether or not the condition is permanent, the status code 404 (Not Found) SHOULD be used instead. 
		/// This response is cacheable unless indicated otherwise.
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public GoneException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The requested resource is no longer available at the server and no forwarding address is known. This condition is expected to be considered 
		/// permanent. Clients with link editing capabilities SHOULD delete references to the Request-URI after user approval. If the server does not 
		/// know, or has no facility to determine, whether or not the condition is permanent, the status code 404 (Not Found) SHOULD be used instead. 
		/// This response is cacheable unless indicated otherwise.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public GoneException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The requested resource is no longer available at the server and no forwarding address is known. This condition is expected to be considered 
		/// permanent. Clients with link editing capabilities SHOULD delete references to the Request-URI after user approval. If the server does not 
		/// know, or has no facility to determine, whether or not the condition is permanent, the status code 404 (Not Found) SHOULD be used instead. 
		/// This response is cacheable unless indicated otherwise.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public GoneException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
