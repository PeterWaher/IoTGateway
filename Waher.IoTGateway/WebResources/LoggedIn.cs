using System;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Script;
using Waher.Security;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Authentication mechanism that makes sure the user has an established session with the IoT Gateway.
	/// </summary>
	public class LoggedIn : HttpAuthenticationScheme
	{
		private readonly HttpServer server;

		/// <summary>
		/// Authentication mechanism that makes sure the user has an established session with the IoT Gateway.
		/// </summary>
		/// <param name="Server">HTTP Server object.</param>
		public LoggedIn(HttpServer Server)
		{
			this.server = Server;
		}

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public override string GetChallenge()
		{
			throw new ForbiddenException("Access denied.");
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <param name="User">User object, if authenticated.</param>
		/// <returns>If the request is authorized.</returns>
		public override bool IsAuthenticated(HttpRequest Request, out IUser User)
		{
			HttpFieldCookie Cookie;
			Variables Variables = Request.Session;
			string HttpSessionID;

			User = null;

			if (Variables is null &&
				(Cookie = Request.Header.Cookie) != null &&
				!string.IsNullOrEmpty(HttpSessionID = Cookie["HttpSessionID"]))
			{
				Request.Session = Variables = this.server.GetSession(HttpSessionID);
			}

			return 
				Variables != null && 
				Variables.TryGetVariable("User", out Variable v) && 
				(User = v.ValueObject as IUser) != null;
		}
	}
}
