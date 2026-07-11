using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Waher.Networking.HTTP.Authentication;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// Abstract base class for OAUTH resources.
	/// </summary>
	public abstract class OAuthResource : HttpSynchronousResource
	{
		/// <summary>
		/// Privilege prefix for OAUTH scopes.
		/// </summary>
		public const string OAuthScopePrivilegePrefix = "OAUTH.Scope.";

		private HttpAuthenticationScheme[]? authenticationSchemes = null;
		private OAuth2Environment environment;

		/// <summary>
		/// OAUTH authorize resource.
		/// </summary>
		/// <param name="Environment">OAuth2 environment.</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthResource(OAuth2Environment Environment, string ResourceName)
			: base(ResourceName)
		{
			this.environment = Environment;
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// OAUTH2 environment, used to access clients, tokens, and other resources.
		/// </summary>
		public OAuth2Environment Environment => this.environment;

		/// <summary>
		/// Data source for users, used to authenticate clients.
		/// </summary>
		public IUserSource? Users => this.environment.UserSource;

		/// <summary>
		/// Realm name, if any, used for authentication. Null if no realm is defined.
		/// </summary>
		public string? Realm => this.environment.Realm;

		/// <summary>
		/// Minimum strength of ciphers used in encryption, if any. 0 if no encryption is used.
		/// </summary>
		public int MinStrength => this.environment.MinStrength;

		/// <summary>
		/// If TLS-encryption is enabled.
		/// </summary>
		public bool Encrypted => this.environment.Encrypted;

		/// <summary>
		/// Available authentication schemes, if initialized.
		/// </summary>
		public HttpAuthenticationScheme[]? AuthenticationSchemes => this.authenticationSchemes;

		/// <summary>
		/// Associated JWT factory object instance, if any is defined.
		/// </summary>
		protected JwtFactory JwtFactory => this.environment.JwtFactory;

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		/// <returns>Array of authentication schemes (possibly empty) available for
		/// authenticating the user making the request. If no default authentication
		/// is to be performed, null can be returned.</returns>
		public override HttpAuthenticationScheme[]? GetAuthenticationSchemes(HttpRequest Request)
		{
			if (Request.Header.Authorization is null)
				return null;

			this.InitAuthentication();

			return this.authenticationSchemes;
		}

		/// <summary>
		/// Initializes authentication schemes, if not already initialized.
		/// </summary>
		/// <returns>If authentication mechanisms have been initialized.</returns>
		protected bool InitAuthentication()
		{
			if (!(this.Users is null))
			{
				this.authenticationSchemes ??= this.CreateAuthenticationSchemes(
					this.JwtFactory, this.Users);
			}

			return !(this.authenticationSchemes is null);
		}

		/// <summary>
		/// Creates a set of authentication scheme object reference for the resource.
		/// </summary>
		/// <param name="JwtFactory">JWT factory used.</param>
		/// <param name="Users">User source used for authentication.</param>
		/// <returns>Set of authentication scheme object references.</returns>
		protected HttpAuthenticationScheme[] CreateAuthenticationSchemes(
			JwtFactory JwtFactory, IUserSource Users)
		{
			// Note: Restricted set of authentication schemes, as compared to
			// HttpModule.GetAuthenticationSchemes().

			List<HttpAuthenticationScheme> Schemes = new List<HttpAuthenticationScheme>();

			if (!(JwtFactory is null))
			{
				Schemes.Add(new JwtAuthentication(this.Encrypted, this.MinStrength,
					this.Realm, Users, JwtFactory));
			}

			HttpServer Server = Types.TryGetModuleParameter<HttpServer>("HTTP");

			if (!(Server is null) && Server.ClientCertificates != ClientCertificates.NotUsed)
				Schemes.Add(new MutualTlsAuthentication(Users));

			Schemes.Add(new BasicAuthentication(this.Encrypted, this.MinStrength,
				this.Realm, Users));

			Schemes.Add(new DigestAuthentication(this.Encrypted, this.MinStrength,
				DigestAlgorithm.MD5, this.Realm, Users));

			Schemes.Add(new DigestAuthentication(this.Encrypted, this.MinStrength,
				DigestAlgorithm.SHA256, this.Realm, Users));

			Schemes.Add(new DigestAuthentication(this.Encrypted, this.MinStrength,
				DigestAlgorithm.SHA3_256, this.Realm, Users));

			if (!(Server is null))
				Schemes.Add(new SessionAuthentication(Server));

			return Schemes.ToArray();
		}

		/// <summary>
		/// Returns default content for an error, for the resource. If returning null, server will choose default content.
		/// </summary>
		/// <param name="StatusCode">Status code.</param>
		/// <returns>Default content, or null if resource lets server choose.</returns>
		public override Task<object> DefaultErrorContent(int StatusCode)
		{
			return Task.FromResult<object>(new Dictionary<string, object>()
			{
				{ "error", "invalid_client" },
				{ "error_description", "Unauthorized access prohibited." }
			});
		}

		/// <summary>
		/// Returns an error back to the client.
		/// </summary>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="ErrorDescription">Error description.</param>
		/// <param name="StatusCode">HTTP status code.</param>
		/// <param name="StatusMessage">HTTP status message.</param>
		protected static Task ReturnError(HttpResponse Response, string ErrorCode,
			string ErrorDescription, int StatusCode, string StatusMessage)
		{
			Response.StatusCode = StatusCode;
			Response.StatusMessage = StatusMessage;
			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");

			return Response.Return(ErrorResponse(ErrorCode, ErrorDescription));
		}

		private static Dictionary<string, object> ErrorResponse(string ErrorCode,
			string ErrorDescription)
		{
			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "error", ErrorCode },
				{ "error_description", ErrorDescription }
			};

			return Result;
		}

		/// <summary>
		/// Returns a Bad Request error back to the client.
		/// </summary>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="ErrorDescription">Error description.</param>
		protected static Task BadRequest(HttpResponse Response, string ErrorCode,
			string ErrorDescription)
		{
			return ReturnError(Response, ErrorCode, ErrorDescription,
				BadRequestException.Code, BadRequestException.StatusMessage);
		}

		/// <summary>
		/// Returns a Forbidden error back to the client.
		/// </summary>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="ErrorDescription">Error description.</param>
		protected static Task Forbidden(HttpResponse Response, string ErrorCode,
			string ErrorDescription)
		{
			return ReturnError(Response, ErrorCode, ErrorDescription,
				ForbiddenException.Code, ForbiddenException.StatusMessage);
		}

		/// <summary>
		/// Returns an Unauthorized error back to the client.
		/// </summary>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="ErrorDescription">Error description.</param>
		/// <param name="Challenges">Authorization challenges.</param>
		protected static Task Unauthorized(HttpResponse Response, string ErrorCode,
			string ErrorDescription, string[] Challenges)
		{
			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");

			return Response.SendResponse(new UnauthorizedException(
				ErrorResponse(ErrorCode, ErrorDescription), Challenges));
		}

		/// <summary>
		/// Returns a Not Found error back to the client.
		/// </summary>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="ErrorDescription">Error description.</param>
		protected static Task NotFound(HttpResponse Response, string ErrorCode,
			string ErrorDescription)
		{
			return ReturnError(Response, ErrorCode, ErrorDescription,
				NotFoundException.Code, NotFoundException.StatusMessage);
		}

		/// <summary>
		/// Returns a Service Unavailable error back to the client.
		/// </summary>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="ErrorDescription">Error description.</param>
		protected static Task ServiceUnavailable(HttpResponse Response, string ErrorCode,
			string ErrorDescription)
		{
			return ReturnError(Response, ErrorCode, ErrorDescription,
				ServiceUnavailableException.Code, ServiceUnavailableException.StatusMessage);
		}

		/// <summary>
		/// Checks if a scope value is valid, according to the OAUTH2 specification.
		/// </summary>
		/// <param name="Scope"></param>
		/// <returns></returns>
		protected static bool IsValidScope(string Scope)
		{
			if (string.IsNullOrEmpty(Scope))
				return false; // For an individual supplied value. Omitted scope is different.

			string[] Tokens = Scope.Split(' ');

			foreach (string Token in Tokens)
			{
				if (Token.Length == 0)
					return false;

				foreach (char ch in Token)
				{
					if (ch == 0x21)
						continue;

					if (ch >= 0x23 && ch <= 0x5B)
						continue;

					if (ch >= 0x5D && ch <= 0x7E)
						continue;

					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks if a user has the privileges associated with a set of scopes.
		/// </summary>
		/// <param name="Scopes">A space-separated list of scopes.</param>
		/// <param name="User">The user to check privileges for.</param>
		/// <param name="MissingPrivilege">Priviliege missing from user.</param>
		/// <returns>True if the user has all the privileges associated with the scopes, 
		/// otherwise false.</returns>
		public static bool HasScopePrivileges(string Scopes, IUser User,
			[NotNullWhen(false)] out string? MissingPrivilege)
		{
			return HasScopePrivileges(
				Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries),
				User, out MissingPrivilege);
		}

		/// <summary>
		/// Checks if a user has the privileges associated with a set of scopes.
		/// </summary>
		/// <param name="Scopes">An array of scopes.</param>
		/// <param name="User">The user to check privileges for.</param>
		/// <param name="MissingPrivilege">Priviliege missing from user.</param>
		/// <returns>True if the user has all the privileges associated with the scopes, 
		/// otherwise false.</returns>
		public static bool HasScopePrivileges(string[] Scopes, IUser User,
			[NotNullWhen(false)] out string? MissingPrivilege)
		{
			foreach (string Scope in Scopes)
			{
				string Privilege = OAuthScopePrivilegePrefix + Scope.Replace(':', '.');
				if (!User.HasPrivilege(Privilege))
				{
					MissingPrivilege = Privilege;
					return false;
				}
			}

			MissingPrivilege = null;
			return true;
		}
	}
}
