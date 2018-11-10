using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The origin server requires the request to be conditional. Intended to prevent "the 'lost update' problem, where a client GETs a resource's state, 
	/// modifies it, and PUTs it back to the server, when meanwhile a third party has modified the state on the server, leading to a conflict.
	/// </summary>
	public class PreconditionRequiredException : HttpException
	{
		private const int Code = 428;
		private const string Msg = "Precondition Required";

		/// <summary>
		/// The origin server requires the request to be conditional. Intended to prevent "the 'lost update' problem, where a client GETs a resource's state, 
		/// modifies it, and PUTs it back to the server, when meanwhile a third party has modified the state on the server, leading to a conflict.
		/// </summary>
		public PreconditionRequiredException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The origin server requires the request to be conditional. Intended to prevent "the 'lost update' problem, where a client GETs a resource's state, 
		/// modifies it, and PUTs it back to the server, when meanwhile a third party has modified the state on the server, leading to a conflict.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public PreconditionRequiredException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The origin server requires the request to be conditional. Intended to prevent "the 'lost update' problem, where a client GETs a resource's state, 
		/// modifies it, and PUTs it back to the server, when meanwhile a third party has modified the state on the server, leading to a conflict.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public PreconditionRequiredException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
