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
		/// <param name="Factory">JWT token factory.</param>
		public JwtAuthentication(string Realm, JwtFactory Factory)
			: this(Realm, null, Factory)
		{
		}

		/// <summary>
		/// Use JWT tokens for authentication. The Bearer scheme defined in RFC 6750 is used:
		/// https://tools.ietf.org/html/rfc6750
		/// </summary>
		/// <param name="Realm">Realm.</param>
		/// <param name="Users">Optional Collection of users to authenticate against.
		/// If no collection is provided, any JWT token created by the factory will
		/// be accepted.</param>
		/// <param name="Factory">JWT token factory.</param>
		public JwtAuthentication(string Realm, IUserSource Users, JwtFactory Factory)
			: base()
		{
			this.realm = Realm;
			this.users = Users;
			this.factory = Factory;
		}

#if WINDOWS_UWP
		/// <summary>
		/// Use JWT tokens for authentication. The Bearer scheme defined in RFC 6750 is used:
		/// https://tools.ietf.org/html/rfc6750
		/// </summary>
		/// <param name="RequireEncryption">If encryption is required.</param>
		/// <param name="Realm">Realm.</param>
		/// <param name="Users">Optional Collection of users to authenticate against.
		/// If no collection is provided, any JWT token created by the factory will
		/// be accepted.</param>
		/// <param name="Factory">JWT token factory.</param>
		public JwtAuthentication(bool RequireEncryption, 
			string Realm, IUserSource Users, JwtFactory Factory)
			: base(RequireEncryption)
#else
		/// <summary>
		/// Use JWT tokens for authentication. The Bearer scheme defined in RFC 6750 is used:
		/// https://tools.ietf.org/html/rfc6750
		/// </summary>
		/// <param name="RequireEncryption">If encryption is required.</param>
		/// <param name="MinStrength">Minimum security strength of algorithms used.</param>
		/// <param name="Realm">Realm.</param>
		/// <param name="Users">Optional Collection of users to authenticate against.
		/// If no collection is provided, any JWT token created by the factory will
		/// be accepted.</param>
		/// <param name="Factory">JWT token factory.</param>
		public JwtAuthentication(bool RequireEncryption, int MinStrength,
			string Realm, IUserSource Users, JwtFactory Factory)
			: base(RequireEncryption, MinStrength)
#endif
		{
			this.realm = Realm;
			this.users = Users;
			this.factory = Factory;
		}

		/// <summary>
		/// Collection of users to authenticate against.
		/// </summary>
		public IUserSource Users => this.users;

		/// <summary>
		/// Realm for authentication
		/// </summary>
		public string Realm => this.realm;

		/// <summary>
		/// JWT Factory
		/// </summary>
		public JwtFactory Factory => this.factory;

		/// <summary>
		/// Gets a challenge for the authenticating client to respond to.
		/// </summary>
		/// <returns>Challenge string.</returns>
		public override string GetChallenge()
		{
			return "Bearer realm=\"" + this.realm + "\"";
		}

		/// <summary>
		/// Gets the access token from an HTTP request.
		/// </summary>
		/// <param name="Request">HTTP Request object.</param>
		/// <returns>Access token, or null if none.</returns>
		public static string GetAccessToken(HttpRequest Request)
		{
			HttpFieldAuthorization Authorization = Request.Header.Authorization;
			if (!(Authorization is null) && Authorization.Value.StartsWith("Bearer ", StringComparison.CurrentCultureIgnoreCase))
				return Authorization.Value.Substring(7).Trim();

			if (Request.Header.TryGetQueryParameter("access_token", out string Token))      // RFC 6750, §2.3: https://www.rfc-editor.org/rfc/rfc6750#section-2.3
				return Token;

			return null;
		}

		/// <summary>
		/// Checks if the request is authorized.
		/// </summary>
		/// <param name="Request">Request object.</param>
		/// <returns>User object, if authenticated, or null otherwise.</returns>
		public override async Task<IUser> IsAuthenticated(HttpRequest Request)
		{
			string TokenStr = GetAccessToken(Request);
			if (string.IsNullOrEmpty(TokenStr))
				return null;

			try
			{
				JwtToken Token = new JwtToken(TokenStr);
				string UserName = Token.Subject;

				if (!this.factory.IsValid(Token, out Reason Reason))
				{
					LoginAuditor.Fail("Login attempt failed. Reason: " + Reason.ToString(), UserName ?? string.Empty, Request.RemoteEndPoint, "HTTP");
					return null;
				}

				if (this.users is null)
				{
					if (string.IsNullOrEmpty(UserName))
						UserName = Request.RemoteEndPoint;

					return new ExternalUser(UserName, Token);
				}
				else
				{
					if (UserName is null)
					{
						LoginAuditor.Fail("Login attempt failed. No user defined.", UserName ?? string.Empty, Request.RemoteEndPoint, "HTTP");
						return null;
					}

					IUser User = await this.users.TryGetUser(UserName);

					if (User is null)
						LoginAuditor.Fail("Login attempt failed.", UserName, Request.RemoteEndPoint, "HTTP");
					else
						await LoginAuditor.SilentSuccess("Login successful.", UserName, Request.RemoteEndPoint, "HTTP");
			
					return User;
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

	}
}
