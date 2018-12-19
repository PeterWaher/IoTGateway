using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.HeaderFields;

namespace Waher.Security.JWT
{
	/// <summary>
	/// Use JWT tokens for authentication. The Bearer scheme defined in RFC 6750 is used:
	/// https://tools.ietf.org/html/rfc6750
	/// </summary>
	public class JwtAuthentication : HttpAuthenticationScheme
	{
		private IUserSource users;
		private JwtFactory factory;
		private string realm;

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
		/// <param name="User">User object, if authenticated.</param>
		/// <returns>If the request is authorized.</returns>
		public override bool IsAuthenticated(HttpRequest Request, out IUser User)
		{
			User = null;

			HttpFieldAuthorization Authorization = Request.Header.Authorization;
			if (Authorization != null && Authorization.Value.StartsWith("Bearer ", StringComparison.CurrentCultureIgnoreCase))
			{
				try
				{
					string TokenStr = Authorization.Value.Substring(7).Trim();
					JwtToken Token = new JwtToken(TokenStr);

					if (!this.factory.IsValid(Token))
						return false;

					if (Token.Subject is null)
						return false;

					return this.users.TryGetUser(Token.Subject, out User);
				}
				catch (Exception)
				{
					return false;
				}
			}

			return false;
		}

	}
}
