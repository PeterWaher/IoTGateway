using System;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Script;
using Waher.Security;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Authentication mechanism that makes sure the user has an established session with the IoT Gateway, and that the user
	/// holds a given set of privileges.
	/// </summary>
	public class RequiredUserPrivileges : HttpAuthenticationScheme
	{
		private readonly HttpServer server;
		private readonly string[] privileges;

		/// <summary>
		/// Authentication mechanism that makes sure the user has an established session with the IoT Gateway, and that the user
		/// holds a given set of privileges.
		/// </summary>
		/// <param name="Server">HTTP Server object.</param>
		/// <param name="Privileges">Required user privileges.</param>
		public RequiredUserPrivileges(HttpServer Server, params string[] Privileges)
		{
			this.server = Server;
			this.privileges = Privileges;
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

			if (Variables is null ||
				!Variables.TryGetVariable("User", out Variable v) ||
				!(v.ValueObject is IUser User))
			{
				return Task.FromResult<IUser>(null);
			}

			foreach (string Privilege in this.privileges)
			{
				if (!User.HasPrivilege(Privilege))
					return Task.FromResult<IUser>(null);
			}

			return Task.FromResult<IUser>(User);
		}
	}
}
