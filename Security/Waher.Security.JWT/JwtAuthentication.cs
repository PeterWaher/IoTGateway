using System;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Security.LoginMonitor;

namespace Waher.Security.JWT
{
	/// <summary>
	/// Use JWT tokens for authentication. The Bearer scheme defined in RFC 6750 is used:
	/// https://tools.ietf.org/html/rfc6750
	/// </summary>
	public class JwtAuthentication : HttpAuthenticationScheme
	{
		private readonly IUserSource users;
		private readonly JwtFactory factory;
		private readonly string realm;

		/// <summary>
		/// Use JWT tokens for authentication. The Bearer scheme defined in RFC 6750 is used:
		/// https://tools.ietf.org/html/rfc6750
		/// </summary>
		/// <param name="Realm">Realm.</param>
		/// <param name="Users">Collection of users to authenticate against.</param>
		/// <param name="Factory">JWT token factory.</param>
		public JwtAuthentication(string Realm, IUserSource Users, JwtFactory Factory)
		{
			this.realm = Realm;
			this.users = Users;
			this.factory = Factory;
		}

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public override string GetChallenge()
		{
			return "Bearer realm=\"" + this.realm + "\"";
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public override async Task<IUser> IsAuthenticated(HttpRequest Request)
		{
			HttpFieldAuthorization Authorization = Request.Header.Authorization;
			if (Authorization != null && Authorization.Value.StartsWith("Bearer ", StringComparison.CurrentCultureIgnoreCase))
			{
				try
				{
					string TokenStr = Authorization.Value.Substring(7).Trim();
					JwtToken Token = new JwtToken(TokenStr);
					string UserName = Token.Subject;

					if (!this.factory.IsValid(Token) || UserName is null)
					{
						LoginAuditor.Fail("Login attempt failed.", UserName ?? string.Empty, Request.RemoteEndPoint, "HTTP");
						return null;
					}

					IUser User = await this.users.TryGetUser(UserName);
					
					if (User is null)
						LoginAuditor.Fail("Login attempt failed.", UserName, Request.RemoteEndPoint, "HTTP");
					else
						LoginAuditor.Success("Login successful.", UserName, Request.RemoteEndPoint, "HTTP");
				
					return User;
				}
				catch (Exception)
				{
					return null;
				}
			}

			return null;
		}

	}
}
