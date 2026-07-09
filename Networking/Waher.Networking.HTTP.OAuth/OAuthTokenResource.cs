using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP.Authentication;
using Waher.Networking.HTTP.ScriptExtensions;
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
		/// <param name="Environment">OAuth2 environment.</param>
		public OAuthTokenResource(OAuth2Environment Environment)
			: this(Environment, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH token resource, as defined in RFC 6749.
		/// </summary>
		/// <param name="Environment">OAuth2 environment.</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthTokenResource(OAuth2Environment Environment, string ResourceName)
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

		internal async Task<string> GenerateTokenCode(IUserWithClaims User, bool Encrypted,
			string CodeChallenge, string CodeChallengeMethod, string RedirectUri,
			string Scope)
		{
			if (this.JwtFactory is null)
				throw new ServiceUnavailableException("No JWT factory configured.");

			string Token = await this.CreateToken(User, Encrypted, Scope);
			string Code = this.GenerateRandomCode();

			codes[Code] = new TokenRef(Token, User, CodeChallenge, CodeChallengeMethod,
				RedirectUri, 3600, Scope);

			return Code;
		}

		/// <summary>
		/// Generates a random unique code.
		/// </summary>
		/// <returns>Random unique code.</returns>
		private string GenerateRandomCode()
		{
			string Code;

			do
			{
				Code = this.Environment.GenerateRandomCode(64);
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
				string CodeChallengeMethod, string RedirectUri, int ExpiresIn, string Scope)
			{
				this.Token = Token;
				this.User = User;
				this.CodeChallenge = CodeChallenge;
				this.CodeChallengeMethod = CodeChallengeMethod;
				this.RedirectUri = RedirectUri;
				this.ExpiresIn = ExpiresIn;
				this.Scope = Scope;
			}

			public string Token;
			public string CodeChallenge;
			public string CodeChallengeMethod;
			public string RedirectUri;
			public string Scope;
			public IUserWithClaims User;
			public int ExpiresIn;

			public async Task<bool> Check(string CodeVerifier, HttpResponse Response)
			{
				switch (this.CodeChallengeMethod)
				{
					case "plain":
						if (CodeVerifier != this.CodeChallenge)
						{
							await Forbidden(Response, "invalid_grant",
								"Invalid code_verifier.");
							return false;
						}
						break;

					case "S256":
						string ExpectedCodeChallenge = Base64Url.Encode(
							Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(CodeVerifier)));

						if (ExpectedCodeChallenge != this.CodeChallenge)
						{
							await Forbidden(Response, "invalid_grant",
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
				await Forbidden(Response, "invalid_grant", "Invalid code.");
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
				Ref.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries),
				this.JwtFactory?.Issuer, true, Ref.User, Request));
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

			if (!Form.TryGetValue("scope", out string Scope))
				Scope = string.Empty;

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
						await Forbidden(Response, "invalid_grant", "Invalid code.");
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
					Scope = Ref.Scope;
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
							Token = await this.CreateToken(UserWithClaims, Request.Encrypted, Scope);
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
							await Forbidden(Response, "invalid_client",
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
								await Forbidden(Response, "invalid_grant",
									"Invalid client_id or client_secret.");
								return;

							case LoginResultType.NoPassword:
								await Forbidden(Response, "invalid_grant",
									"No or empty client_secret.");
								return;

							case LoginResultType.TemporarilyBlocked:
								await Forbidden(Response, "invalid_grant",
									"Temporarily blocked. Try again after: " +
									LoginResult.Next?.ToString());
								return;

							case LoginResultType.PermanentlyBlocked:
								await Forbidden(Response, "invalid_client",
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
						Token = await this.CreateToken(UserWithClaims, Request.Encrypted, Scope);
					}
					else
					{
						await BadRequest(Response, "invalid_request",
							"Missing credentials.");
						return;
					}

					if (!HasScopePrivileges(Scope, User, out string? MissingPrivilege))
					{
						await Forbidden(Response, "access_denied",
							"User lacks privilege: " + MissingPrivilege);
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

					if (string.IsNullOrEmpty(Scope))
						Scope = (TokenFamily.Scopes?.Length ?? 0) == 0 ? string.Empty : string.Join(' ', TokenFamily.Scopes);
					else
					{
						string[] NewScopes = Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);

						foreach (string Scope2 in NewScopes)
						{
							if (Array.IndexOf(TokenFamily.Scopes, Scope2) < 0)
							{
								await Forbidden(Response, "invalid_scope",
									"Not permitted to escalate scope.");
								return;
							}
						}

						TokenFamily.Scopes = NewScopes;
					}

					refreshTokens.Remove(RefreshToken);
					usedRefreshTokens.Add(RefreshToken, TokenFamily);

					User = TokenFamily.User;
					Token = await this.CreateToken(User, Request.Encrypted, Scope);
					break;

				case OAuthDeviceAuthorizationResource.GrantType:
					if (!this.Environment.HasDeviceAuthorizationResource)
					{
						await ServiceUnavailable(Response, "server_error",
							"Device authorization not configured.");
						return;
					}

					if (!Form.TryGetValue("device_code", out string DeviceCode))
					{
						await BadRequest(Response, "invalid_request", "Missing device_code.");
						return;
					}

					if (!Form.TryGetValue("client_id", out ClientId))
					{
						await BadRequest(Response, "invalid_request", "Missing client_id.");
						return;
					}

					if (!this.Environment.DeviceAuthorizationResource.TryGetDeviceReference(
						DeviceCode, out OAuthDeviceAuthorizationResource.DeviceRef? DeviceReference))
					{
						await Forbidden(Response, "expired_token", "Invalid device_code, or token has expired.");
						return;
					}

					if (ClientId != DeviceReference.Device.UserName)
					{
						await Forbidden(Response, "access_denied", "Invalid client_id.");
						return;
					}

					DateTime TP = DateTime.UtcNow;

					if (DeviceReference.LastPoll.HasValue &&
						TP.Subtract(DeviceReference.LastPoll.Value).TotalSeconds <
						OAuthDeviceAuthorizationResource.MinimumIntervalSeconds)
					{
						await BadRequest(Response, "slow_down", "Polling too fast. Slow down.");
						return;
					}

					DeviceReference.LastPoll = TP;

					if (!DeviceReference.Result.HasValue)
					{
						await BadRequest(Response, "authorization_pending", "Authorization has not yet been granted by owner.");
						return;
					}

					if (!DeviceReference.Result.Value)
					{
						await Forbidden(Response, "access_denied", "Access has been denied by owner.");
						return;
					}

					if (!HasScopePrivileges(Scope, DeviceReference.Owner, out MissingPrivilege))
					{
						await Forbidden(Response, "access_denied",
							"Owner lacks privilege: " + MissingPrivilege);
						return;
					}

					User = DeviceReference.Device;
					Scope = DeviceReference.Scope;

					Token = await this.CreateToken(User, Request.Encrypted, Scope);

					DeviceReference.Remove();
					break;

				default:
					await BadRequest(Response, "unsupported_grant_type",
						"Unsupported grant_type: " + GrantType);
					return;
			}

			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");

			await Response.Return(this.TokenResponse(Token, null, 3600,
				Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries),
				this.JwtFactory?.Issuer, IssueRefreshToken, User, Request, TokenFamily));
		}

		private async Task<string> CreateToken(IUserWithClaims User, bool Encrypted, 
			string Scope)
		{
			if (this.JwtFactory is null)
				throw new ServiceUnavailableException("No JWT factory configured.");

			return await CreateToken(User, Encrypted, this.JwtFactory, Scope);
		}

		internal static async Task<string> CreateToken(IUserWithClaims User, bool Encrypted, 
			JwtFactory JwtFactory, string Scope)
		{
			if (string.IsNullOrEmpty(Scope))
				return await User.CreateToken(JwtFactory, Encrypted);
			else
			{
				return await User.CreateToken(JwtFactory, Encrypted,
					new KeyValuePair<string, object>(JwtClaims.Scope, Scope));
			}
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

			string PasswordHash = BasicAuthentication.ComputePasswordHash(UserName,
				Realm, Password, User.PasswordHashType, out byte? HashBytes);

			string ExpectedHash = User.PasswordHash;
			if (HashBytes.HasValue)
				ExpectedHash = DigestAuthentication.EnsureHex(ExpectedHash, HashBytes.Value);

			if (PasswordHash == ExpectedHash)
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
			string? State, int ExpiresIn, string[] Scopes, string? Issuer,
			bool IssueRefreshToken, IUserWithClaims User, HttpRequest Request)
		{
			return this.TokenResponse(Token, State, ExpiresIn, Scopes, Issuer,
				IssueRefreshToken, User, Request, null);
		}

		private Dictionary<string, object> TokenResponse(string Token,
			string? State, int ExpiresIn, string[] Scopes, string? Issuer,
			bool IssueRefreshToken, IUserWithClaims User, HttpRequest Request,
			TokenFamily? TokenFamily)
		{
			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "access_token", Token },
				{ "token_type", "Bearer" },
				{ "expires_in", ExpiresIn },
				{ "scope", string.Join(' ', Scopes) }
			};

			if (!string.IsNullOrEmpty(State))
				Result["state"] = State;

			if (!string.IsNullOrEmpty(Issuer))
				Result["iss"] = Issuer;

			if (IssueRefreshToken)
			{
				if (TokenFamily is null)
					TokenFamily = new TokenFamily(Token, Scopes, User, Request);
				else if (!TokenFamily.CanUseRefreshToken(User.UserName, Request))
					throw new ForbiddenException();
				else
					TokenFamily.Add(Token);

				string RefreshToken = this.GenerateRandomCode();
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
			public string[] Scopes { get; set; }
			public bool HasRemoteCertificate { get; }
			public string RemoteEndpoint { get; }
			public string RemoteCertificateSerialNumber { get; }

			public TokenFamily(string Token, string[] Scopes, IUserWithClaims User,
				HttpRequest FirstRequest)
			{
				this.User = User;
				this.tokens = new ChunkedList<string>() { Token };
				this.FirstRequest = FirstRequest;
				this.Scopes = Scopes;
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
