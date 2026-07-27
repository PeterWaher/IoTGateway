using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.IO;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH introspection resource, as defined in RFCs 7662.
	/// https://datatracker.ietf.org/doc/html/rfc7662
	/// </summary>
	public class OAuthIntrospectionResource : OAuthResource, IHttpPostMethod
	{
		/// <summary>
		/// Privilege for OAUTH introspection.
		/// </summary>
		public const string OAuthIntrospectionPrivilege = "OAUTH.Introspection";

		/// <summary>
		/// Default introspection resource path: /oauth/introspect
		/// </summary>
		public const string DefaultResourcePath = "/oauth/introspect";

		/// <summary>
		/// OAUTH introspection resource, as defined in RFCs 7662.
		/// </summary>
		/// <param name="Environment">OAuth2 environment.</param>
		public OAuthIntrospectionResource(OAuth2Environment Environment)
			: this(Environment, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH introspection resource, as defined in RFCs 7662.
		/// </summary>
		/// <param name="Environment">OAuth2 environment.</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthIntrospectionResource(OAuth2Environment Environment,
			string ResourceName)
			: base(Environment, ResourceName)
		{
			Environment.Register(this);
		}

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		/// <returns>Array of authentication schemes (possibly empty) available for
		/// authenticating the user making the request. If no default authentication
		/// is to be performed, null can be returned.</returns>
		public override HttpAuthenticationScheme[]? GetAuthenticationSchemes(HttpRequest Request)
		{
			this.InitAuthentication();

			return this.AuthenticationSchemes;
		}

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.HasData)
			{
				await BadRequest(Response, "invalid_request", "Missing payload.");
				return;
			}

			if (!Request.User?.HasPrivilege(OAuthIntrospectionPrivilege) ?? false)
			{
				await Response.SendResponse(ForbiddenException.AccessDenied(this.ResourceName,
					Request.RemoteEndPoint.RemovePortNumber(), OAuthIntrospectionPrivilege));
				return;
			}

			ContentResponse Decoded = await Request.DecodeDataAsync();
			if (Decoded.HasError)
			{
				await Response.SendResponse(Decoded.Error);
				return;
			}

			if (!(Decoded.Decoded is Dictionary<string, string> Form))
			{
				await BadRequest(Response, "invalid_request", "Expected form data.");
				return;
			}

			if (!Form.TryGetValue("token", out string Token))
			{
				await BadRequest(Response, "invalid_request", "Missing token.");
				return;
			}

			if (!this.Environment.HasTokenResource ||
				!this.Environment.TokenResource.TryGetTokenType(Token, out JwtToken? ParsedToken,
				out OAuthTokenType? TokenType))
			{
				ParsedToken = null;
				TokenType = null;
			}

			Dictionary<string, object> Result = new Dictionary<string, object>();

			if (!(ParsedToken is null))
			{
				foreach (KeyValuePair<string, object> P in ParsedToken.Claims)
				{
					Result[P.Key] = P.Value;

					if (P.Key == JwtClaims.Subject)
					{
						if (P.Value is string UserName &&
							!(await this.Environment.UserSource.TryGetUser(UserName) is null))
						{
							Result["username"] = UserName;
						}
					}
				}
			}

			if (TokenType == OAuthTokenType.AccessToken ||
				TokenType == OAuthTokenType.ExpiredAccessToken)
			{
				Result["token_type"] = "Bearer";
			}
			else
				Result["token_type"] = "N_A";

			Result["active"] = TokenType.HasValue && (
				TokenType == OAuthTokenType.AccessToken ||
				TokenType == OAuthTokenType.RefreshToken);

		}
	}
}
