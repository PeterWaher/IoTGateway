using System;
using System.Threading.Tasks;
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
		/// If the authentication scheme uses user sessions.
		/// </summary>
		public virtual bool UserSessions => false;

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public abstract string GetChallenge();

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public abstract Task<IUser> IsAuthenticated(HttpRequest Request);
	}
}
