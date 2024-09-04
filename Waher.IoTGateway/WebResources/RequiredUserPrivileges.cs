using System.Threading.Tasks;
using System.Web;
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
		private readonly string userVariable;
		private readonly string loginPage;

		/// <summary>
		/// Authentication mechanism that makes sure the user has an established session with the IoT Gateway, and that the user
		/// holds a given set of privileges.
		/// </summary>
		/// <param name="Server">HTTP Server object.</param>
		/// <param name="Privileges">Required user privileges.</param>
		public RequiredUserPrivileges(HttpServer Server, params string[] Privileges)
			: this("User", string.Empty, Server, Privileges)
		{
		}

		/// <summary>
		/// Authentication mechanism that makes sure the user has an established session with the IoT Gateway, and that the user
		/// holds a given set of privileges.
		/// </summary>
		/// <param name="UserVariable">Name of user variable.</param>
		/// <param name="Server">HTTP Server object.</param>
		/// <param name="Privileges">Required user privileges.</param>
		public RequiredUserPrivileges(string UserVariable, HttpServer Server, params string[] Privileges)
			: this(UserVariable, string.Empty, Server, Privileges)
		{
		}

		/// <summary>
		/// Authentication mechanism that makes sure the user has an established session with the IoT Gateway, and that the user
		/// holds a given set of privileges.
		/// </summary>
		/// <param name="UserVariable">Name of user variable.</param>
		/// <param name="LoginPage">Login page.</param>
		/// <param name="Server">HTTP Server object.</param>
		/// <param name="Privileges">Required user privileges.</param>
		public RequiredUserPrivileges(string UserVariable, string LoginPage, HttpServer Server, params string[] Privileges)
		{
			this.userVariable = UserVariable;
			this.loginPage = LoginPage;
			this.server = Server;
			this.privileges = Privileges;
		}

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public override string GetChallenge()
		{
			foreach (string Privilege in this.privileges)
				throw ForbiddenException.AccessDenied(string.Empty, string.Empty, Privilege);

			throw ForbiddenException.AccessDenied(string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public override Task<IUser> IsAuthenticated(HttpRequest Request)
		{
			Variables Variables = Request.Session;
			string HttpSessionID;

			if (Variables is null &&
				!string.IsNullOrEmpty(HttpSessionID = HttpResource.GetSessionId(Request, Request.Response)))
			{
				Request.Session = Variables = this.server.GetSession(HttpSessionID);
			}

			if (Variables is null ||
				!Variables.TryGetVariable(this.userVariable, out Variable v) ||
				!(v.ValueObject is IUser User))
			{
				if (!string.IsNullOrEmpty(this.loginPage))
					throw new SeeOtherException(this.loginPage + "?from=" + HttpUtility.UrlEncode(Request.Header.GetURL(true, true)));

				return Task.FromResult<IUser>(null);
			}

			foreach (string Privilege in this.privileges)
			{
				if (!User.HasPrivilege(Privilege))
					return Task.FromResult<IUser>(null);
			}

			return Task.FromResult(User);
		}
	}
}
