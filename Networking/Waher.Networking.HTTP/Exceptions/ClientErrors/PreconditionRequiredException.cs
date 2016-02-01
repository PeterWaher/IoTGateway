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
		/// <summary>
		/// The origin server requires the request to be conditional. Intended to prevent "the 'lost update' problem, where a client GETs a resource's state, 
		/// modifies it, and PUTs it back to the server, when meanwhile a third party has modified the state on the server, leading to a conflict.
		/// </summary>
		public PreconditionRequiredException()
			: base(428, "Precondition Required")
		{
		}
	}
}
