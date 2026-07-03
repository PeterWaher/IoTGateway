using System;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.OAuth.Events
{
	/// <summary>
	/// Event arguments for implicit OAUTH authentication requests.
	/// </summary>
	public class ImplicitAuthenticationEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for implicit OAUTH authentication requests.
		/// </summary>
		/// <param name="Request">HTTP Request object instance from client making 
		/// the request.</param>
		public ImplicitAuthenticationEventArgs(HttpRequest Request)
		{
			this.Request = Request;
		}

		/// <summary>
		/// HTTP Request object instance from client making the request.
		/// </summary>
		public HttpRequest Request { get; }

		/// <summary>
		/// If implicit authentication can use result of WWW-Authenticate mechanism
		/// in HTTP to identify a user, and to generate a token from that user
		/// identity. (Default is true.)
		/// </summary>
		public bool PermitWwwAuthentication { get; set; } = true;

		/// <summary>
		/// If user identity can be implicitly authenticated using the client certificate
		/// used in mutual TLS authentication, and to generate a token from that user identity.
		/// (Default is true.)
		/// </summary>
		public bool PermitMtlsAuthentication { get; set; } = true;

		/// <summary>
		/// Implicitly authenticated user, if any. If null, no user was authenticated.
		/// </summary>
		public IUserWithClaims? User { get; set; } = null;
	}
}
