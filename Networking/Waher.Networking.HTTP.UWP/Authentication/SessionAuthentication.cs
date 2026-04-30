using System;
using System.Threading.Tasks;
using Waher.Script;
using Waher.Security;

namespace Waher.Networking.HTTP.Authentication
{
	/// <summary>
	/// Authentication mechanism that makes sure the user has an established session with 
	/// the web server.
	/// </summary>
	public class SessionAuthentication : HttpAuthenticationScheme
	{
		/// <summary>
		/// Default user variable: User
		/// </summary>
		public const string DefaultUserVariable = "User";

		private readonly HttpServer server;
		private readonly string userVariable;

		/// <summary>
		/// Authentication mechanism that makes sure the user has an established session
		/// with the web server.
		/// </summary>
		/// <param name="Server">HTTP Server object.</param>
		public SessionAuthentication(HttpServer Server)
			: this(DefaultUserVariable, Server)
		{
		}

		/// <summary>
		/// Authentication mechanism that makes sure the user has an established session
		/// with the web server.
		/// </summary>
		/// <param name="UserVariable">Name of user variable.</param>
		/// <param name="Server">HTTP Server object.</param>
		public SessionAuthentication(string UserVariable, HttpServer Server)
		{
			this.userVariable = UserVariable;
			this.server = Server;
		}

		/// <summary>
		/// Gets available challenges for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge strings.</returns>
		public override string[] GetChallenges()
		{
			return Array.Empty<string>();
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public override Task<IUser> IsAuthenticated(HttpRequest Request)
		{
			SessionVariables Variables = Request.Session;
			string HttpSessionID;

			if (Variables is null &&
				!string.IsNullOrEmpty(HttpSessionID = HttpResource.GetSessionId(Request, Request.Response)))
			{
				Request.Session = Variables = this.server.GetSession(HttpSessionID);
			}

			if (!(Variables is null) &&
				Variables.TryGetVariable(this.userVariable, out Variable v) &&
				v.ValueObject is IUser User)
			{
				return Task.FromResult(User);
			}
			else
				return Task.FromResult<IUser>(null);
		}
	}
}
