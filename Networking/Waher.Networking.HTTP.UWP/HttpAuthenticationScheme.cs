using System;
using System.Collections.Generic;
using System.Text;
using Waher.Security;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Base class for all HTTP authentication schemes, as defined in RFC-7235:
	/// https://datatracker.ietf.org/doc/rfc7235/
	/// </summary>
	public abstract class HttpAuthenticationScheme
	{
		/// <summary>
		/// Base class for all HTTP authentication schemes, as defined in RFC-7235:
		/// https://datatracker.ietf.org/doc/rfc7235/
		/// </summary>
		public HttpAuthenticationScheme()
		{ 
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="User">User object, if authenticated.</param>
		/// <returns>If the request is authorized.</returns>
		public abstract bool IsAuthenticated(HttpRequest Request, out IUser User);

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public abstract string GetChallenge();
	}
}
