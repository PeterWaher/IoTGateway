using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The origin server requires the request to be conditional. Intended to prevent "the 'lost update' problem, where a client GETs a resource's state, 
	/// modifies it, and PUTs it back to the server, when meanwhile a third party has modified the state on the server, leading to a conflict.
	/// </summary>
	public class PreconditionRequiredException : HttpException
	{
		/// <summary>
		/// 428
		/// </summary>
		public const int Code = 428;

		/// <summary>
		/// Precondition Required
		/// </summary>
		public const string StatusMessage = "Precondition Required";

		/// <summary>
		/// The origin server requires the request to be conditional. Intended to prevent "the 'lost update' problem, where a client GETs a resource's state, 
		/// modifies it, and PUTs it back to the server, when meanwhile a third party has modified the state on the server, leading to a conflict.
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public PreconditionRequiredException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The origin server requires the request to be conditional. Intended to prevent "the 'lost update' problem, where a client GETs a resource's state, 
		/// modifies it, and PUTs it back to the server, when meanwhile a third party has modified the state on the server, leading to a conflict.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public PreconditionRequiredException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The origin server requires the request to be conditional. Intended to prevent "the 'lost update' problem, where a client GETs a resource's state, 
		/// modifies it, and PUTs it back to the server, when meanwhile a third party has modified the state on the server, leading to a conflict.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public PreconditionRequiredException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
