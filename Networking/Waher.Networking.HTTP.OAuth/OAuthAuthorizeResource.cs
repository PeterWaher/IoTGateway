using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.HTTP.Authentication;
using Waher.Networking.HTTP.OAuth.Events;
using Waher.Runtime.Collections;
using Waher.Security.JWT;
using Waher.Security.LoginMonitor;
using Waher.Security.Users;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH authorize resource, as defined in RFC 6749.
	/// https://datatracker.ietf.org/doc/html/rfc6749
	/// </summary>
	public class OAuthAuthorizeResource : OAuthResource, IHttpGetMethod, IHttpPostMethod
	{
		/// <summary>
		/// Default authorize resource path: /oauth/authorize
		/// </summary>
		public const string DefaultResourcePath = "/oauth/authorize";

		/// <summary>
		/// OAUTH authorize resource, as defined in RFC 6749.
		/// </summary>
		/// <param name="Environment">OAuth2 environment.</param>
		public OAuthAuthorizeResource(OAuth2Environment Environment)
			: this(Environment, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH authorize resource, as defined in RFC 6749.
		/// </summary>
		/// <param name="Environment">OAuth2 environment.</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthAuthorizeResource(OAuth2Environment Environment, string ResourceName)
			: base(Environment, ResourceName)
		{
			Environment.Register(this);
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Reference to token resource.
		/// </summary>
		internal OAuthTokenResource TokenResource => this.Environment.TokenResource;

		/// <summary>
		/// Reference to registration resource.
		/// </summary>
		internal OAuthRegistrationResource OAuthRegistrationResource => this.Environment.RegistrationResource;

		/// <summary>
		/// Reference to device authorization resource.
		/// </summary>
		internal OAuthDeviceAuthorizationResource OAuthDeviceAuthorizationResource => this.Environment.DeviceAuthorizationResource;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			if (HasDuplicateQueryParameters(Request))
			{
				await BadRequest(Response, "invalid_request",
						"Duplicate query parameters.");
				return;
			}

			if (!Request.Header.TryGetQueryParameter("response_type", out string ResponseType))
			{
				await BadRequest(Response, "invalid_request",
						"Missing response_type parameter.");
				return;
			}

			await this.PrepareForm(ResponseType, Request.Header.QueryParametersPerName,
				Request, Response);
		}

		internal static bool HasDuplicateQueryParameters(HttpRequest Request)
		{
			HashSet<string> Parameters = new HashSet<string>();

			foreach (KeyValuePair<string, string> P in Request.Header.QueryParameters)
			{
				if (Parameters.Contains(P.Key))
					return true;
				else
					Parameters.Add(P.Key);
			}

			return false;
		}

		private async Task PrepareForm(string ResponseType, IDictionary<string, string> Form,
			HttpRequest Request, HttpResponse Response)
		{
			if (!Form.TryGetValue("state", out string State))
				State = string.Empty;

			if (!Form.TryGetValue("scope", out string Scope))
				Scope = string.Empty;

			switch (ResponseType)
			{
				case "code":        // Authorization Code
					if (!Form.TryGetValue("client_id", out string ClientId))
						ClientId = string.Empty;

					if (!Form.TryGetValue("redirect_uri", out string RedirectUri) ||
						string.IsNullOrEmpty(RedirectUri))
					{
						await BadRequest(Response, "invalid_request",
							"Missing or empty redirect_uri parameter.");
						return;
					}

					if (!Form.TryGetValue("code_challenge", out string CodeChallenge))
						CodeChallenge = string.Empty;

					if (!Form.TryGetValue("code_challenge_method", out string CodeChallengeMethod) ||
						string.IsNullOrEmpty(CodeChallengeMethod))
					{
						CodeChallengeMethod = "plain";
					}

					if (CodeChallengeMethod != "plain" && CodeChallengeMethod != "S256")
					{
						await BadRequest(Response, "invalid_request",
							"Unsupported code_challenge_method: " + CodeChallengeMethod);
						return;
					}

					Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
					Response.SetHeader("Pragma", "no-cache");

					await Response.Return(await this.GenerateLoginForm(Request, Response,
						ClientId, RedirectUri, State, Scope, CodeChallenge, CodeChallengeMethod,
						string.Empty));
					return;

				case "token":       // Implicit
					if (this.JwtFactory is null)
					{
						await ServiceUnavailable(Response, "server_error",
							"JWT Factory not available.");
						return;
					}

					ImplicitAuthenticationEventArgs e = new ImplicitAuthenticationEventArgs(Request);
					await this.ImplicitAuthenticationRequest.Raise(this, e);

					IUserWithClaims? User = e.User;

					if (User is null &&
						(e.PermitWwwAuthentication || e.PermitMtlsAuthentication) &&
						Request.User is IUserWithClaims UserWithClaims)
					{
						User = UserWithClaims;
					}

					if (!(User is null))
					{
						if (Form.TryGetValue("client_id", out ClientId) &&
							ClientId != User.UserName)
						{
							LoginAuditor.Fail("Credentials mismatch. User name in request: " +
								ClientId + ", user name in authenticated user: " + User.UserName,
								User.UserName, Request.RemoteEndPoint, "OAUTH");

							await Forbidden(Response, "invalid_request", "Invalid credentials.");
							return;
						}

						string[] Scopes = Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
						if (!OAuthTokenResource.HasScopePrivileges(Scopes, User, out string? MissingPrivilege))
						{
							await Forbidden(Response, "access_denied",
								"User lacks privilege: " + MissingPrivilege);
							return;
						}

						Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
						Response.SetHeader("Pragma", "no-cache");

						string Token = await OAuthTokenResource.CreateToken(User, 
							Request.Encrypted, this.JwtFactory, Scope);

						await Response.Return(this.TokenResource.TokenResponse(Token, State,
							3600, Scopes, this.JwtFactory?.Issuer, false, User, Request));
						return;
					}

					if (!this.InitAuthentication())
					{
						await ServiceUnavailable(Response, "server_error",
							"Authentication not enabled.");
					}
					else
					{
						ChunkedList<string>? Challenges = null;

						foreach (HttpAuthenticationScheme AuthenticationScheme in this.AuthenticationSchemes!)
						{
							if (AuthenticationScheme is MutualTlsAuthentication)
							{
								if (!e.PermitMtlsAuthentication)
									continue;
							}
							else
							{
								if (!e.PermitWwwAuthentication)
									continue;
							}

							Challenges ??= new ChunkedList<string>();
							Challenges.AddRange(AuthenticationScheme.GetChallenges(Request));
						}

						if (Challenges is null)
							await Forbidden(Response, "access_denied", "Access denied");
						else
						{
							await Unauthorized(Response, "access_denied", "Access denied",
								Challenges.ToArray());
						}
					}
					return;

				default:
					if (string.IsNullOrEmpty(ResponseType))
					{
						await BadRequest(Response, "invalid_request",
							"Empty response_type.");
						return;
					}
					else
					{
						await BadRequest(Response, "unsupported_response_type",
							"Unsupported response_type parameter: " + ResponseType);
					}
					return;
			}
		}

		/// <summary>
		/// Event raised when an implicit authentication request is received.
		/// </summary>
		public event EventHandlerAsync<ImplicitAuthenticationEventArgs>? ImplicitAuthenticationRequest = null;

		private async Task<HtmlDocument> GenerateLoginForm(HttpRequest Request,
			HttpResponse Response, string UserName, string From, string State,
			string Scope, string CodeChallenge, string CodeChallengeMethod,
			string ErrorMessage)
		{
			StringBuilder Markdown = new StringBuilder();

			Markdown.AppendLine("Title: Login");
			Markdown.AppendLine("Description: OAUTH login page.");

			if (Request.Server.TryGetLocalResourceFileName("/Master.md", Request.Host, out string FileName) &&
				File.Exists(FileName))
			{
				Markdown.AppendLine("Master: /Master.md");
			}

			Markdown.Append("Date: ");
			Markdown.AppendLine(CommonTypes.EncodeRfc822(DateTime.UtcNow));
			Markdown.AppendLine();
			Markdown.AppendLine(new string('=', 40));
			Markdown.AppendLine();

			Markdown.AppendLine("Login");
			Markdown.AppendLine("========");
			Markdown.AppendLine();

			Markdown.Append("<form id='LoginForm' action='");
			Markdown.Append(this.ResourceName);
			Markdown.Append("' method='post'>");
			Markdown.Append("<input type='hidden' name='redirect_uri' value='");
			Markdown.Append(XML.HtmlAttributeEncode(From));
			Markdown.AppendLine("'/>");
			Markdown.Append("<input type='hidden' name='state' value='");
			Markdown.Append(XML.HtmlAttributeEncode(State));
			Markdown.AppendLine("'/>");
			Markdown.Append("<input type='hidden' name='scope' value='");
			Markdown.Append(XML.HtmlAttributeEncode(Scope));
			Markdown.AppendLine("'/>");
			Markdown.Append("<input type='hidden' name='code_challenge' value='");
			Markdown.Append(XML.HtmlAttributeEncode(CodeChallenge));
			Markdown.AppendLine("'/>");
			Markdown.Append("<input type='hidden' name='code_challenge_method' value='");
			Markdown.Append(XML.HtmlAttributeEncode(CodeChallengeMethod));
			Markdown.AppendLine("'/>");
			Markdown.AppendLine();

			Markdown.AppendLine("<p>");
			Markdown.AppendLine("<label for='client_id'>User Name:</label>  ");
			Markdown.Append("<input id='client_id' name='client_id' type='text' autofocus autocomplete='username");

			if (!string.IsNullOrEmpty(UserName))
			{
				Markdown.Append("' value='");
				Markdown.Append(XML.HtmlAttributeEncode(UserName));
			}

			Markdown.AppendLine("'/>");
			Markdown.AppendLine("</p>");
			Markdown.AppendLine();

			Markdown.AppendLine("<p>");
			Markdown.AppendLine("<label for='client_secret'>Password:</label>  ");
			Markdown.Append("<input id='client_secret' name='client_secret' type='password' ");
			Markdown.AppendLine("autocomplete='current-password'/>");
			Markdown.AppendLine("</p>");
			Markdown.AppendLine();

			if (!string.IsNullOrEmpty(ErrorMessage))
			{
				Markdown.AppendLine("<p>");
				Markdown.Append("<strong id='errorMessage'>");
				Markdown.Append(XML.HtmlValueEncode(ErrorMessage));
				Markdown.AppendLine("</strong>");
				Markdown.AppendLine("</p>");
				Markdown.AppendLine();
			}

			Markdown.AppendLine("<button type='submit'>Login</button>");
			Markdown.AppendLine("</form>");

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown.ToString());
			string Html = await Doc.GenerateHTML();

			Response.SetHeader("X-Frame-Options", "DENY");
			Response.SetHeader("Content-Security-Policy", "frame-ancestors 'none'; default-src 'self'; script-src 'self'; object-src 'none'; base-uri 'none'; form-action 'self'");

			return new HtmlDocument(Html);
		}

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

			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError || !(Content.Decoded is Dictionary<string, string> Form))
			{
				await BadRequest(Response, "invalid_request",
					"Expected URL-encoded WWW form.");
				return;
			}

			if (Form.TryGetValue("response_type", out string ResponseType))
			{
				await this.PrepareForm(ResponseType, Form, Request, Response);
				return;
			}

			if (!Form.TryGetValue("client_id", out string UserName) ||
				!Form.TryGetValue("client_secret", out string Password) ||
				!Form.TryGetValue("redirect_uri", out string RedirectUri) ||
				!Form.TryGetValue("state", out string State) ||
				!Form.TryGetValue("scope", out string Scope) ||
				!Form.TryGetValue("code_challenge", out string CodeChallenge))
			{
				await BadRequest(Response, "invalid_request", "Invalid form.");
				return;
			}

			if (!Form.TryGetValue("code_challenge_method", out string CodeChallengeMethod) ||
				string.IsNullOrWhiteSpace(CodeChallengeMethod))
			{
				CodeChallengeMethod = "plain";
			}

			if (CodeChallengeMethod != "plain" && CodeChallengeMethod != "S256")
			{
				await BadRequest(Response, "invalid_request",
					"Unsupported code_challenge_method: " + CodeChallengeMethod);
				return;
			}

			if (string.IsNullOrEmpty(RedirectUri))
			{
				await BadRequest(Response, "invalid_request",
					"Missing or empty redirect_uri parameter.");
				return;
			}

			this.InitAuthentication();

			LoginResult? LoginResult = await OAuthTokenResource.DoLogin(UserName, Password,
				this.Users!, Request, this.Realm ?? string.Empty);

			if (LoginResult is null)
			{
				await Forbidden(Response, "access_denied",
					"User cannot authenticate via this interface.");
				return;
			}

			switch (LoginResult.Type)
			{
				case LoginResultType.Success:
					Request.User = LoginResult.User;

					if (!(LoginResult.User is IUserWithClaims UserWithClaims))
					{
						await Response.Return(await this.GenerateLoginForm(Request,
							Response, UserName, RedirectUri, State, Scope, CodeChallenge,
							CodeChallengeMethod, "User cannot be used with OAUTH login."));
						return;
					}

					string Code = await this.TokenResource.GenerateTokenCode(UserWithClaims,
						Request.Encrypted, CodeChallenge, CodeChallengeMethod, RedirectUri,
						Scope);

					if (RedirectUri.Contains('?'))
						RedirectUri += "&code=" + HttpUtility.UrlEncode(Code);
					else
						RedirectUri += "?code=" + HttpUtility.UrlEncode(Code);

					if (!string.IsNullOrEmpty(State))
						RedirectUri += "&state=" + HttpUtility.UrlEncode(State);

					if (this.JwtFactory?.HasIssuer ?? false)
						RedirectUri += "&iss=" + HttpUtility.UrlEncode(this.JwtFactory.Issuer);

					await Response.SendResponse(new SeeOtherException(RedirectUri));
					break;

				case LoginResultType.InvalidCredentials:
				default:
					await Response.Return(await this.GenerateLoginForm(Request, Response,
						UserName, RedirectUri, State, Scope, CodeChallenge, CodeChallengeMethod,
						"Invalid user name or password."));
					return;

				case LoginResultType.NoPassword:
					await Forbidden(Response, "access_denied", "Password empty.");
					return;

				case LoginResultType.TemporarilyBlocked:
					await Response.Return(await this.GenerateLoginForm(Request, Response,
						UserName, RedirectUri, State, Scope, CodeChallenge, CodeChallengeMethod,
						"You are temporarily blocked. Try again after: " +
						LoginResult.Next?.ToString()));
					return;

				case LoginResultType.PermanentlyBlocked:
					await Response.Return(await this.GenerateLoginForm(Request, Response,
						UserName, RedirectUri, State, Scope, CodeChallenge, CodeChallengeMethod,
						"You are permanently blocked."));
					return;
			}
		}

	}
}
