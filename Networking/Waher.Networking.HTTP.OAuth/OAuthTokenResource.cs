using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP.Authentication;
using Waher.Runtime.Cache;
using Waher.Runtime.Collections;
using Waher.Runtime.IO;
using Waher.Security;
using Waher.Security.JWT;
using Waher.Security.LoginMonitor;
using Waher.Security.Users;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH token resource, as defined in RFC 6749.
	/// https://datatracker.ietf.org/doc/html/rfc6749
	/// </summary>
	public class OAuthTokenResource : OAuthResource, IHttpGetMethod, IHttpPostMethod
	{
		/// <summary>
		/// Default token resource path: /oauth/token
		/// </summary>
		public const string DefaultResourcePath = "/oauth/token";

		private static readonly Cache<string, TokenRef> codes = new Cache<string, TokenRef>(int.MaxValue, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
		private static readonly Cache<string, TokenFamily> refreshTokens = new Cache<string, TokenFamily>(int.MaxValue, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
		private static readonly Cache<string, TokenFamily> usedRefreshTokens = new Cache<string, TokenFamily>(int.MaxValue, TimeSpan.FromHours(1), TimeSpan.FromHours(1));

		/// <summary>
		/// OAUTH token resource, as defined in RFC 6749.
		/// </summary>
		public OAuthTokenResource()
			: this(null, null, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH token resource, as defined in RFC 6749.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthTokenResource(JwtFactory? JwtFactory)
			: this(null, JwtFactory, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH token resource, as defined in RFC 6749.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthTokenResource(string ResourceName)
			: this(null, null, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH token resource, as defined in RFC 6749.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthTokenResource(JwtFactory? JwtFactory, string ResourceName)
			: this(null, JwtFactory, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH token resource, as defined in RFC 6749.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		public OAuthTokenResource(IUserSource? UserSource)
			: this(UserSource, null, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH token resource, as defined in RFC 6749.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthTokenResource(IUserSource? UserSource, JwtFactory? JwtFactory)
			: this(UserSource, JwtFactory, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH token resource, as defined in RFC 6749.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthTokenResource(IUserSource? UserSource, string ResourceName)
			: this(UserSource, null, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH token resource, as defined in RFC 6749.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthTokenResource(IUserSource? UserSource, JwtFactory? JwtFactory,
			string ResourceName)
			: base(UserSource, JwtFactory, ResourceName)
		{
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		internal async Task<string> GenerateTokenCode(IUserWithClaims User, bool Encrypted,
			string CodeChallenge, string CodeChallengeMethod, string RedirectUri)
		{
			if (this.JwtFactory is null)
				throw new ServiceUnavailableException("No JWT factory configured.");

			string Token = await User.CreateToken(this.JwtFactory, Encrypted);
			string Code = this.GenerateRandomCode(64);

			codes[Code] = new TokenRef(Token, User, CodeChallenge, CodeChallengeMethod,
				RedirectUri, 3600);

			return Code;
		}

		/// <summary>
		/// Generates a random unique code.
		/// </summary>
		/// <param name="NrBytes">Number of bytes of random.</param>
		/// <returns>Random unique code.</returns>
		protected override string GenerateRandomCode(int NrBytes)
		{
			string Code;

			do
			{
				Code = base.GenerateRandomCode(NrBytes);
			}
			while (
				codes.ContainsKey(Code) ||
				refreshTokens.ContainsKey(Code) ||
				usedRefreshTokens.ContainsKey(Code));

			return Code;
		}

		private class TokenRef
		{
			public TokenRef(string Token, IUserWithClaims User, string CodeChallenge,
				string CodeChallengeMethod, string RedirectUri, int ExpiresIn)
			{
				this.Token = Token;
				this.User = User;
				this.CodeChallenge = CodeChallenge;
				this.CodeChallengeMethod = CodeChallengeMethod;
				this.RedirectUri = RedirectUri;
				this.ExpiresIn = ExpiresIn;
			}

			public string Token;
			public string CodeChallenge;
			public string CodeChallengeMethod;
			public string RedirectUri;
			public IUserWithClaims User;
			public int ExpiresIn;

			public async Task<bool> Check(string CodeVerifier, HttpResponse Response)
			{
				switch (this.CodeChallengeMethod)
				{
					case "plain":
						if (CodeVerifier != this.CodeChallenge)
						{
							await Forbidden(Response, "access_denied", 
								"Invalid code_verifier.");
							return false;
						}
						break;

					case "S256":
						string ExpectedCodeChallenge = Base64Url.Encode(
							Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(CodeVerifier)));

						if (ExpectedCodeChallenge != this.CodeChallenge)
						{
							await Forbidden(Response, "access_denied", 
								"Invalid code_verifier.");
							return false;
						}
						break;

					default:
						await BadRequest(Response, "invalid_request", 
							"Unsupported code_challenge_method: " + this.CodeChallengeMethod);
						return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			if (OAuthAuthorizeResource.HasDuplicateQueryParameters(Request))
			{
				await BadRequest(Response, "invalid_request",
					"Duplicate query parameters.");
				return;
			}

			if (!Request.Header.TryGetQueryParameter("code", out string Code))
			{
				await BadRequest(Response, "invalid_request", "Missing code.");
				return;
			}

			if (!codes.TryGetValue(Code, out TokenRef Ref))
			{
				await Forbidden(Response, "access_denied", "Invalid code.");
				return;
			}

			if (!string.IsNullOrEmpty(Ref.CodeChallenge))
			{
				if (!Request.Header.TryGetQueryParameter("code_verifier", out string CodeVerifier))
				{
					await BadRequest(Response, "invalid_request", "Missing code_verifier.");
					return;
				}

				if (!await Ref.Check(CodeVerifier, Response))
					return;
			}

			codes.Remove(Code);

			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");

			await Response.Return(this.TokenResponse(Ref.Token, null, Ref.ExpiresIn,
				string.Empty, this.JwtFactory?.Issuer, true, Ref.User, Request));
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			if (this.JwtFactory is null)
			{
				await ServiceUnavailable(Response, "server_error",
					"No JWT factory configured.");
				return;
			}

			if (!Request.HasData)
			{
				await BadRequest(Response, "invalid_request", "No payload in request.");
				return;
			}

			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError || !(Content.Decoded is Dictionary<string, string> Form))
			{
				await BadRequest(Response, "invalid_request", 
					"Expected URL-encoded WWW form.");
				return;
			}

			if (!Form.TryGetValue("grant_type", out string GrantType))
			{
				await BadRequest(Response, "invalid_request", "Missing grant_type.");
				return;
			}

			string ClientId;
			string Token;
			IUserWithClaims User;
			TokenFamily? TokenFamily = null;
			bool IssueRefreshToken = true;

			switch (GrantType)
			{
				case "authorization_code":
					if (!Form.TryGetValue("code", out string Code))
					{
						await BadRequest(Response, "invalid_request", "Missing code.");
						return;
					}

					if (!codes.TryGetValue(Code, out TokenRef Ref))
					{
						await Forbidden(Response, "access_denied", "Invalid code.");
						return;
					}

					if (!Form.TryGetValue("redirect_uri", out string RedirectUri) ||
						!Form.TryGetValue("client_id", out ClientId))
					{
						await BadRequest(Response, "invalid_request", "Missing client_id.");
						return;
					}

					if (ClientId != Ref.User.UserName)
					{
						await Forbidden(Response, "access_denied", "Access denied");
						return;
					}

					if (Ref.RedirectUri != RedirectUri)
					{
						await Forbidden(Response, "access_denied", "Access denied");
						return;
					}

					if (!string.IsNullOrEmpty(Ref.CodeChallenge))
					{
						if (!Form.TryGetValue("code_verifier", out string CodeVerifier))
						{
							await BadRequest(Response, "invalid_request", "Missing code_verifier.");
							return;
						}

						if (!await Ref.Check(CodeVerifier, Response))
							return;
					}

					codes.Remove(Code);
					Token = Ref.Token;
					User = Ref.User;
					break;

				case "client_credentials":
				case "password":
					string ClientSecret = string.Empty;
					bool HasCredentials;

					if (GrantType == "password")
					{
						HasCredentials = Form.TryGetValue("username", out ClientId) &&
							Form.TryGetValue("password", out ClientSecret);
					}
					else
					{
						IssueRefreshToken = false;

						if (Request.User is null)
						{
							HasCredentials = Form.TryGetValue("client_id", out ClientId) &&
								Form.TryGetValue("client_secret", out ClientSecret);
						}
						else
						{
							if (Form.ContainsKey("client_id") ||
								Form.ContainsKey("client_secret"))
							{
								await BadRequest(Response, "invalid_request", 
									"Invalid request parameters.");
								return;
							}

							if (!(Request.User is IUserWithClaims UserWithClaims))
							{
								await Response.SendResponse(ForbiddenException.AccessDenied(
									this.ResourceName, Request.RemoteEndPoint));
								return;
							}

							User = UserWithClaims;
							Token = await UserWithClaims.CreateToken(this.JwtFactory, Request.Encrypted);
							break;
						}
					}

					if (HasCredentials)
					{
						if (!Request.Encrypted && (Request.Server.OpenHttpsPorts?.Length ?? 0) > 0)
						{
							await Forbidden(Response, "invalid_request", 
								"Request must be performed over an encrypted connection.");
							return;
						}

						if (Request.Encrypted && Request.CipherStrength < 128)
						{
							await Forbidden(Response, "invalid_request", 
								"Cipher strength too weak.");
							return;
						}

						this.InitAuthentication();

						LoginResult? LoginResult = await DoLogin(ClientId, ClientSecret,
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
								break;

							case LoginResultType.InvalidCredentials:
							default:
								await Forbidden(Response, "access_denied",
									"Invalid client_id or client_secret.");
								return;

							case LoginResultType.NoPassword:
								await Forbidden(Response, "access_denied",
									"No or empty client_secret.");
								return;

							case LoginResultType.TemporarilyBlocked:
								await Forbidden(Response, "access_denied",
									"Temporarily blocked. Try again after: " +
									LoginResult.Next?.ToString());
								return;

							case LoginResultType.PermanentlyBlocked:
								await Forbidden(Response, "access_denied",
									"Permanently blocked.");
								return;
						}

						if (!(Request.User is IUserWithClaims UserWithClaims))
						{
							await Response.SendResponse(ForbiddenException.AccessDenied(
								this.ResourceName, Request.RemoteEndPoint));
							return;
						}

						User = UserWithClaims;
						Token = await UserWithClaims.CreateToken(this.JwtFactory, Request.Encrypted);
					}
					else
					{
						await BadRequest(Response, "invalid_request", 
							"Missing credentials.");
						return;
					}
					break;

				case "refresh_token":
					if (!Form.TryGetValue("refresh_token", out string RefreshToken))
					{
						await BadRequest(Response, "invalid_request", 
							"Missing refresh_token.");
						return;
					}

					if (!refreshTokens.TryGetValue(RefreshToken, out TokenFamily))
					{
						if (usedRefreshTokens.TryGetValue(RefreshToken, out TokenFamily))
						{
							string Message = "Attempt to reuse refresh token. Has the token leaked? Deprecating all associated tokens.";

							LoginAuditor.Fail(Message, TokenFamily.User.UserName,
								Request.RemoteEndPoint, "OAUTH");

							Log.Alert(Message, TokenFamily.User.UserName,
								Request.RemoteEndPoint, "TokenLeakage",
								await LoginAuditor.Annotate(Request.RemoteEndPoint));

							foreach (string Token2 in TokenFamily.Tokens)
							{
								if (JwtToken.TryParse(Token2, out JwtToken ParsedToken))
									JwtFactory.Deprecate(ParsedToken);
							}

							usedRefreshTokens.Remove(RefreshToken);
						}

						await Forbidden(Response, "access_denied", 
							"Invalid refresh_token.");
						return;
					}

					if (!Form.TryGetValue("client_id", out ClientId))
					{
						await BadRequest(Response, "invalid_request", "Missing client_id.");
						return;
					}

					if (!TokenFamily.CanUseRefreshToken(ClientId, Request))
					{
						await Forbidden(Response, "access_denied", "Access denied");
						return;
					}

					refreshTokens.Remove(RefreshToken);
					usedRefreshTokens.Add(RefreshToken, TokenFamily);

					User = TokenFamily.User;
					Token = await User.CreateToken(this.JwtFactory, Request.Encrypted);
					break;

				default:
					await BadRequest(Response, "invalid_request", 
						"Unsupported grant_type: " + GrantType);
					return;
			}

			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");

			await Response.Return(this.TokenResponse(Token, null, 3600, string.Empty,
				this.JwtFactory?.Issuer, IssueRefreshToken, User, Request, TokenFamily));
		}

		internal static async Task<LoginResult?> DoLogin(string UserName, string Password,
			IUserSource Users, HttpRequest Request, string Realm)
		{
			if (string.IsNullOrEmpty(Password))
				return new LoginResult();

			if (!(Request.Server.LoginAuditor is null))
			{
				DateTime? Next = await Request.Server.LoginAuditor.GetEarliestLoginOpportunity(
					Request.RemoteEndPoint, "OAUTH");

				if (Next.HasValue)
					return new LoginResult(Next.Value);
			}

			IUser User = await Users.TryGetUser(UserName);
			if (User is null)
			{
				LoginAuditor.Fail("Login attempt using invalid user name.", UserName, Request.RemoteEndPoint, "OAUTH",
					new KeyValuePair<string, object>("UserName", UserName));
				return new LoginResult(User);
			}

			string ExpectedHash = User.PasswordHash;

			switch (User.PasswordHashType)
			{
				case "":
					break;

				case "Internal":
					Password = DigestAuthentication.ToHex(Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(UserName + ":" + Password)));
					ExpectedHash = DigestAuthentication.EnsureHex(ExpectedHash, 32);
					break;

				case "DIGEST-MD5":
					Password = DigestAuthentication.ToHex(DigestAuthentication.H_MD5(UserName + ":" + Realm + ":" + Password));
					ExpectedHash = DigestAuthentication.EnsureHex(ExpectedHash, 16);
					break;

				case "DIGEST-SHA-256":
					Password = DigestAuthentication.ToHex(DigestAuthentication.H_SHA256(UserName + ":" + Realm + ":" + Password));
					ExpectedHash = DigestAuthentication.EnsureHex(ExpectedHash, 32);
					break;

				case "DIGEST-SHA3-256":
					Password = DigestAuthentication.ToHex(DigestAuthentication.H_SHA3_256(UserName + ":" + Realm + ":" + Password));
					ExpectedHash = DigestAuthentication.EnsureHex(ExpectedHash, 32);
					break;

				default:
					return null;
			}

			if (Password == ExpectedHash)
			{
				LoginAuditor.Success("Login successful.", UserName, Request.RemoteEndPoint, "HTTP");
				return new LoginResult(User);
			}
			else
			{
				LoginAuditor.Fail("Login attempt failed.", UserName, Request.RemoteEndPoint, "HTTP");
				return new LoginResult(null);
			}
		}

		internal Dictionary<string, object> TokenResponse(string Token,
			string? State, int ExpiresIn, string Scope, string? Issuer,
			bool IssueRefreshToken, IUserWithClaims User, HttpRequest Request)
		{
			return this.TokenResponse(Token, State, ExpiresIn, Scope, Issuer,
				IssueRefreshToken, User, Request, null);
		}

		private Dictionary<string, object> TokenResponse(string Token,
			string? State, int ExpiresIn, string Scope, string? Issuer,
			bool IssueRefreshToken, IUserWithClaims User, HttpRequest Request,
			TokenFamily? TokenFamily)
		{
			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "access_token", Token },
				{ "token_type", "Bearer" },
				{ "expires_in", ExpiresIn },
				{ "scope", Scope }
			};

			if (!string.IsNullOrEmpty(State))
				Result["state"] = State;

			if (!string.IsNullOrEmpty(Issuer))
				Result["iss"] = Issuer;

			if (IssueRefreshToken)
			{
				if (TokenFamily is null)
					TokenFamily = new TokenFamily(Token, User, Request);
				else if (!TokenFamily.CanUseRefreshToken(User.UserName, Request))
					throw new ForbiddenException();
				else
					TokenFamily.Add(Token);

				string RefreshToken = this.GenerateRandomCode(64);
				refreshTokens[RefreshToken] = TokenFamily;

				Result["refresh_token"] = RefreshToken;
			}

			return Result;
		}

		private class TokenFamily
		{
			private readonly ChunkedList<string> tokens;

			public IEnumerable<string> Tokens => this.tokens;
			public IUserWithClaims User { get; }
			public HttpRequest FirstRequest { get; }
			public bool HasRemoteCertificate { get; }
			public string RemoteEndpoint { get; }
			public string RemoteCertificateSerialNumber { get; }

			public TokenFamily(string Token, IUserWithClaims User, HttpRequest FirstRequest)
			{
				this.User = User;
				this.tokens = new ChunkedList<string>() { Token };
				this.FirstRequest = FirstRequest;
				this.HasRemoteCertificate = !(this.FirstRequest.RemoteCertificate is null);
				this.RemoteEndpoint = this.FirstRequest.RemoteEndPoint.RemovePortNumber();
				this.RemoteCertificateSerialNumber = 
					this.FirstRequest.RemoteCertificate?.GetSerialNumberString()
					?? string.Empty;
			}

			public void Add(string Token) => this.tokens.Add(Token);

			public bool CanUseRefreshToken(string ClientId, HttpRequest Request)
			{
				if (ClientId != this.User.UserName)
					return false;

				if (this.HasRemoteCertificate)
				{
					if (Request.RemoteCertificate is null)
						return false;

					if (Request.RemoteCertificate.GetSerialNumberString() !=
						this.RemoteCertificateSerialNumber)
					{
						return false;
					}
				}
				else
				{
					if (this.RemoteEndpoint != Request.RemoteEndPoint.RemovePortNumber())
						return false;
				}

				return true;
			}
		}
	}
}
