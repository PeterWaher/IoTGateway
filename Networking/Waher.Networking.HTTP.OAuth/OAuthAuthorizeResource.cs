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
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Security.JWT;
using Waher.Security.Users;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH authorize resource.
	/// </summary>
	public class OAuthAuthorizeResource : HttpSynchronousResource,
		IHttpGetMethod, IHttpPostMethod
	{
		private readonly OAuthTokenResource tokenResource;
		private HttpAuthenticationScheme[]? authenticationSchemes = null;
		private JwtFactory? jwtFactory;

		/// <summary>
		/// OAUTH authorize resource.
		/// </summary>
		/// <param name="TokenResource">OAuth token resource.</param>
		public OAuthAuthorizeResource(OAuthTokenResource TokenResource)
			: this(TokenResource, null)
		{
		}

		/// <summary>
		/// OAUTH authorize resource.
		/// </summary>
		/// <param name="TokenResource">OAuth token resource.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthAuthorizeResource(OAuthTokenResource TokenResource, JwtFactory? JwtFactory)
			: this(TokenResource, JwtFactory, "/oauth/authorize")
		{
		}

		/// <summary>
		/// OAUTH authorize resource.
		/// </summary>
		/// <param name="TokenResource">OAuth token resource.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthAuthorizeResource(OAuthTokenResource TokenResource, JwtFactory? JwtFactory,
			string ResourceName)
			: base(ResourceName)
		{
			this.tokenResource = TokenResource;
			this.jwtFactory = JwtFactory;
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
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		/// <returns>Array of authentication schemes (possibly empty) available for
		/// authenticating the user making the request. If no default authentication
		/// is to be performed, null can be returned.</returns>
		public override HttpAuthenticationScheme[]? GetAuthenticationSchemes(HttpRequest Request)
		{
			if (this.jwtFactory is null)
			{
				if (Types.TryGetModuleParameter("JWT", out JwtFactory JwtFactory) &&
					!JwtFactory.Disposed)
				{
					this.jwtFactory = JwtFactory;
					this.authenticationSchemes = null;
				}
			}

			if (Request.Header.Authorization is null)
				return null;

			this.authenticationSchemes ??= OAuthTokenResource.CreateAuthenticationSchemes(this.jwtFactory);

			return this.authenticationSchemes;
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.Header.TryGetQueryParameter("response_type", out string ResponseType))
			{
				await Response.SendResponse(new BadRequestException("Missing response_type parameter."));
				return;
			}

			switch (ResponseType)
			{
				case "code":
					if (!Request.Header.TryGetQueryParameter("client_id", out string ClientId))
						ClientId = string.Empty;

					if (!Request.Header.TryGetQueryParameter("redirect_uri", out string RedirectUri) ||
						string.IsNullOrEmpty(RedirectUri))
					{
						await Response.SendResponse(new BadRequestException("Missing or empty redirect_uri parameter."));
						return;
					}

					if (!Request.Header.TryGetQueryParameter("state", out string State))
						State = string.Empty;

					if (!Request.Header.TryGetQueryParameter("code_challenge", out string CodeChallenge))
						CodeChallenge = string.Empty;

					if (!Request.Header.TryGetQueryParameter("code_challenge_method", out string CodeChallengeMethod))
						CodeChallengeMethod = string.Empty;

					await Response.Return(await this.GenerateLoginForm(Request, ClientId,
						HttpUtility.UrlDecode(RedirectUri), 
						HttpUtility.UrlDecode(State), 
						HttpUtility.UrlDecode(CodeChallenge), 
						HttpUtility.UrlDecode(CodeChallengeMethod), 
						string.Empty));
					return;

				case "token":
					if (this.jwtFactory is null)
					{
						await Response.SendResponse(new ServiceUnavailableException("No JWT factory configured."));
						return;
					}

					if (Request.User is IUserWithClaims UserWithClaims)
					{
						string Token = await UserWithClaims.CreateToken(this.jwtFactory, Request.Encrypted);
						await Response.Return(OAuthTokenResource.TokenResponse(Token));
						return;
					}

					this.authenticationSchemes ??= OAuthTokenResource.CreateAuthenticationSchemes(this.jwtFactory);

					if ((this.authenticationSchemes?.Length ?? 0) == 0)
						await Response.SendResponse(new ForbiddenException());
					else
					{
						ChunkedList<string> Challenges = new ChunkedList<string>();

						foreach (HttpAuthenticationScheme AuthenticationScheme in this.authenticationSchemes!)
							Challenges.AddRange(AuthenticationScheme.GetChallenges());

						await Response.SendResponse(new UnauthorizedException(Challenges.ToArray()));
					}
					return;

				default:
					await Response.SendResponse(new BadRequestException("Unsupported response_type parameter: " + ResponseType));
					return;
			}
		}

		private async Task<HtmlDocument> GenerateLoginForm(HttpRequest Request,
			string UserName, string From, string State, string CodeChallenge,
			string CodeChallengeMethod, string ErrorMessage)
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
			Markdown.Append("<input type='hidden' name='From' value='");
			Markdown.Append(XML.HtmlAttributeEncode(From));
			Markdown.AppendLine("'/>");
			Markdown.Append("<input type='hidden' name='State' value='");
			Markdown.Append(XML.HtmlAttributeEncode(State));
			Markdown.AppendLine("'/>");
			Markdown.Append("<input type='hidden' name='CodeChallenge' value='");
			Markdown.Append(XML.HtmlAttributeEncode(CodeChallenge));
			Markdown.AppendLine("'/>");
			Markdown.Append("<input type='hidden' name='CodeChallengeMethod' value='");
			Markdown.Append(XML.HtmlAttributeEncode(CodeChallengeMethod));
			Markdown.AppendLine("'/>");
			Markdown.AppendLine();

			Markdown.AppendLine("<p>");
			Markdown.AppendLine("<label for='UserName'>User Name:</label>  ");
			Markdown.Append("<input name='UserName' type='text' autocomplete='username");

			if (!string.IsNullOrEmpty(UserName))
			{
				Markdown.Append("' value='");
				Markdown.Append(XML.HtmlAttributeEncode(UserName));
			}

			Markdown.AppendLine("'/>");
			Markdown.AppendLine("</p>");
			Markdown.AppendLine();

			Markdown.AppendLine("<p>");
			Markdown.AppendLine("<label for='Password'>Password:</label>  ");
			Markdown.Append("<input name='Password' type='password' ");
			Markdown.AppendLine("autocomplete='current-password' autofocus/>");
			Markdown.AppendLine("</p>");
			Markdown.AppendLine();

			if (!string.IsNullOrEmpty(ErrorMessage))
			{
				Markdown.AppendLine("<p>");
				Markdown.Append("<strong>");
				Markdown.Append(XML.HtmlValueEncode(ErrorMessage));
				Markdown.AppendLine("</strong>");
				Markdown.AppendLine("</p>");
				Markdown.AppendLine();
			}

			Markdown.AppendLine("<button type='submit'>Login</button>");
			Markdown.AppendLine("</form>");

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown.ToString());
			string Html = await Doc.GenerateHTML();

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
				await Response.SendResponse(new BadRequestException("Missing payload."));
				return;
			}

			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError || !(Content.Decoded is Dictionary<string, string> Form))
			{
				await Response.SendResponse(new BadRequestException("Expected URL-encoded WWW form."));
				return;
			}

			if (Form.Count != 6 ||
				!Form.TryGetValue("UserName", out string UserName) ||
				!Form.TryGetValue("Password", out string Password) ||
				!Form.TryGetValue("From", out string From) ||
				!Form.TryGetValue("State", out string State) ||
				!Form.TryGetValue("CodeChallenge", out string CodeChallenge) ||
				!Form.TryGetValue("CodeChallengeMethod", out string CodeChallengeMethod))
			{
				await Response.SendResponse(new BadRequestException("Invalid form."));
				return;
			}

			if (string.IsNullOrEmpty(From))
			{
				await Response.SendResponse(new BadRequestException("Missing or empty From parameter."));
				return;
			}

			string Nonce = Guid.NewGuid().ToString();
			byte[] PasswordHash = Users.ComputeHash(UserName, Password);
			string PasswordNonceHash = Convert.ToBase64String(
				Hashes.ComputeHMACSHA256Hash(Encoding.UTF8.GetBytes(Nonce),
				PasswordHash));

			LoginResult LoginResult = await Users.Login(UserName, PasswordNonceHash,
				Nonce, Request.RemoteEndPoint, "OAuth2");

			switch (LoginResult.Type)
			{
				case LoginResultType.Success:
					Request.User = LoginResult.User;

					if (!(LoginResult.User is IUserWithClaims UserWithClaims))
					{
						await Response.Return(await this.GenerateLoginForm(Request, UserName,
							From, State, CodeChallenge, CodeChallengeMethod,
							"User cannot be used with OAUTH login."));
						return;
					}

					string Code = await this.tokenResource.GenerateTokenCode(UserWithClaims,
						Request.Encrypted, CodeChallenge, CodeChallengeMethod);

					if (From.Contains('?'))
						From += "&code=" + HttpUtility.UrlEncode(Code);
					else
						From += "?code=" + HttpUtility.UrlEncode(Code);

					if (!string.IsNullOrEmpty(State))
						From += "&state=" + HttpUtility.UrlEncode(State);

					await Response.SendResponse(new SeeOtherException(From));
					break;

				case LoginResultType.InvalidCredentials:
				default:
					await Response.Return(await this.GenerateLoginForm(Request, UserName,
						From, State, CodeChallenge, CodeChallengeMethod, 
						"Invalid user name or password."));
					return;

				case LoginResultType.NoPassword:
					await Response.SendResponse(new ForbiddenException(
						"Password empty."));
					return;

				case LoginResultType.TemporarilyBlocked:
					await Response.Return(await this.GenerateLoginForm(Request, UserName,
						From, State, CodeChallenge, CodeChallengeMethod, 
						"You are temporarily blocked. Try again after: " +
						LoginResult.Next?.ToString()));
					return;

				case LoginResultType.PermanentlyBlocked:
					await Response.Return(await this.GenerateLoginForm(Request, UserName,
						From, State, CodeChallenge, CodeChallengeMethod, 
						"You are permanently blocked."));
					return;
			}
		}

	}
}
