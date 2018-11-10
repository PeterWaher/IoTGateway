using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The precondition given in one or more of the request-header fields evaluated to false when it was tested on the server. This response code 
	/// allows the client to place preconditions on the current resource metainformation (header field data) and thus prevent the requested method 
	/// from being applied to a resource other than the one intended. 
	/// </summary>
	public class PreconditionFailedException : HttpException
	{
		private const int Code = 411;
		private const string Msg = "Precondition Failed";

		/// <summary>
		/// The precondition given in one or more of the request-header fields evaluated to false when it was tested on the server. This response code 
		/// allows the client to place preconditions on the current resource metainformation (header field data) and thus prevent the requested method 
		/// from being applied to a resource other than the one intended. 
		/// </summary>
		public PreconditionFailedException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The precondition given in one or more of the request-header fields evaluated to false when it was tested on the server. This response code 
		/// allows the client to place preconditions on the current resource metainformation (header field data) and thus prevent the requested method 
		/// from being applied to a resource other than the one intended. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public PreconditionFailedException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The precondition given in one or more of the request-header fields evaluated to false when it was tested on the server. This response code 
		/// allows the client to place preconditions on the current resource metainformation (header field data) and thus prevent the requested method 
		/// from being applied to a resource other than the one intended. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public PreconditionFailedException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
