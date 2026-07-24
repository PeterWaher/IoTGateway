using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
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
		private static bool permitAccessTokenInQueryString = false;

		private readonly IUserSource users;
		private readonly JwtFactory factory;
		private readonly string realm;
		private readonly Uri resourceMetaData;

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
			this.resourceMetaData = null;
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
			this.resourceMetaData = null;
		}

		/// <summary>
		/// Use JWT tokens for authentication. The Bearer scheme defined in RFC 6750 is used:
		/// https://tools.ietf.org/html/rfc6750
		/// </summary>
		/// <param name="Realm">Realm.</param>
		/// <param name="Factory">JWT token factory.</param>
		/// <param name="ResourceMetaData">URI pointing to resource meta-data the
		/// client can read to understand how it can authenticate itself to gain
		/// access.</param>
		public JwtAuthentication(string Realm, JwtFactory Factory, Uri ResourceMetaData)
			: this(Realm, null, Factory, ResourceMetaData)
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
		/// <param name="ResourceMetaData">URI pointing to resource meta-data the
		/// client can read to understand how it can authenticate itself to gain
		/// access.</param>
		public JwtAuthentication(string Realm, IUserSource Users, JwtFactory Factory,
			Uri ResourceMetaData)
			: base()
		{
			this.realm = Realm;
			this.users = Users;
			this.factory = Factory;
			this.resourceMetaData = ResourceMetaData;
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
		/// <param name="ResourceMetaData">URI pointing to resource meta-data the
		/// client can read to understand how it can authenticate itself to gain
		/// access.</param>
		public JwtAuthentication(bool RequireEncryption,
			string Realm, IUserSource Users, JwtFactory Factory,
			Uri ResourceMetaData)
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
		/// <param name="ResourceMetaData">URI pointing to resource meta-data the
		/// client can read to understand how it can authenticate itself to gain
		/// access.</param>
		public JwtAuthentication(bool RequireEncryption, int MinStrength,
			string Realm, IUserSource Users, JwtFactory Factory,
			Uri ResourceMetaData)
			: base(RequireEncryption, MinStrength)
#endif
		{
			this.realm = Realm;
			this.users = Users;
			this.factory = Factory;
			this.resourceMetaData = ResourceMetaData;
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
		/// URI pointing to resource meta-data the client can read to understand how it 
		/// can authenticate itself to gain access.
		/// </summary>
		public Uri ResourceMetaData => this.resourceMetaData;

		/// <summary>
		/// Gets available challenges for the authenticating client to respond to.
		/// </summary>
		/// <param name="Request">HTTP Request object.</param>
		/// <returns>Challenge strings.</returns>
		public override string[] GetChallenges(HttpRequest Request)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Bearer realm=\"");
			sb.Append(this.realm);
			sb.Append('"');

			if (Request.TryGetMetaData("BearerError", out object Value) &&
				Value is KeyValuePair<string, string> Error)
			{
				sb.Append(", error=\"");
				sb.Append(Error.Key);
				sb.Append("\", error_description=\"");
				sb.Append(Error.Value);
				sb.Append('"');
			}

			if (!(this.resourceMetaData is null))
			{
				sb.Append(", resource_metadata=\"");
				sb.Append(this.resourceMetaData.ToString());
				sb.Append('"');
			}

			return new string[] { sb.ToString() };
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

			if (permitAccessTokenInQueryString)
			{
				if (Request.Header.TryGetQueryParameter("access_token", out string Token))      // RFC 6750, §2.3: https://www.rfc-editor.org/rfc/rfc6750#section-2.3
					return Token;
			}

			return null;
		}

		/// <summary>
		/// If true, access tokens are permitted in query strings. Default is false, since
		/// this is not recommended.
		/// </summary>
		public bool PermitAccessTokenInQueryString
		{
			get => permitAccessTokenInQueryString;
			set
			{
				if (permitAccessTokenInQueryString != value)
				{
					if (value)
						Log.Warning("Access tokens should not be used in query strings by default.");

					permitAccessTokenInQueryString = value;
				}
			}
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
				if (!JwtToken.TryParse(TokenStr, out JwtToken Token))
				{
					Request.AddMetaData("BearerError", new KeyValuePair<string, string>(
						"invalid_token", "Unable to parse token."));

					return null;
				}

				string UserName = Token.Subject;

				if (!this.factory.IsValid(Token, out Reason Reason))
				{
					string Description;

					switch (Reason)
					{
						case Reason.NoAlgorithm:
							Description = "No algorithm specified in token.";
							break;

						case Reason.UnsupportedAlgorithm:
							Description = "Unsupported algorithm specified in token.";
							break;

						case Reason.NoSignature:
							Description = "No signature found in token.";
							break;

						case Reason.Expired:
							Description = "Token has expired.";
							break;

						case Reason.TooEarly:
							Description = "Token is not yet valid.";
							break;

						case Reason.InvalidSignature:
							Description = "Invalid signature in token.";
							break;

						case Reason.Deprecated:
							Description = "Token uses deprecated algorithm.";
							break;

						default:
							Description = "Reason: " + Reason.ToString();
							break;
					}

					Request.AddMetaData("BearerError", new KeyValuePair<string, string>(
						"invalid_token", Description));

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
						Request.AddMetaData("BearerError", new KeyValuePair<string, string>(
							"invalid_token", "No used defined."));

						LoginAuditor.Fail("Login attempt failed. No user defined.", string.Empty, Request.RemoteEndPoint, "HTTP");
						return null;
					}

					IUser User = await this.users.TryGetUser(UserName);

					if (User is null)
					{
						Request.AddMetaData("BearerError", new KeyValuePair<string, string>(
							"invalid_token", "User not valid in this context."));

						LoginAuditor.Fail("Login attempt failed.", UserName, Request.RemoteEndPoint, "HTTP");
					}
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
