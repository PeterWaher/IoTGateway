using System;
using System.Threading.Tasks;
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
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public override Task<IUser> IsAuthenticated(HttpRequest Request)
		{
			HttpFieldCookie Cookie;
			Variables Variables = Request.Session;
			string HttpSessionID;

			if (Variables is null &&
				(Cookie = Request.Header.Cookie) != null &&
				!string.IsNullOrEmpty(HttpSessionID = Cookie["HttpSessionID"]))
			{
				Request.Session = Variables = this.server.GetSession(HttpSessionID);
			}

			if (Variables != null && Variables.TryGetVariable("User", out Variable v))
				return Task.FromResult<IUser>(v.ValueObject as IUser);
			else
				return Task.FromResult<IUser>(null);
		}
	}
}
