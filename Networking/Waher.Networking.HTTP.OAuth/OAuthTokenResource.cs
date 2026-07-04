using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP.Authentication;
using Waher.Runtime.Cache;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Security.JWT;
using Waher.Security.LoginMonitor;
using Waher.Security.Users;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH token resource.
	/// </summary>
	public class OAuthTokenResource : HttpSynchronousResource,
		IHttpGetMethod, IHttpPostMethod
	{
		/// <summary>
		/// Default token resource path: /oauth/token
		/// </summary>
		public const string DefaultResourcePath = "/oauth/token";

		private static readonly Cache<string, TokenRef> tokenCache = new Cache<string, TokenRef>(int.MaxValue, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();
		private readonly IUserSource userSource;
		private HttpAuthenticationScheme[]? authenticationSchemes = null;
		private JwtFactory? jwtFactory;
		private string? realm;

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		public OAuthTokenResource()
			: this(null, null, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthTokenResource(JwtFactory? JwtFactory)
			: this(null, JwtFactory, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthTokenResource(string ResourceName)
			: this(null, null, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthTokenResource(JwtFactory? JwtFactory, string ResourceName)
			: this(null, JwtFactory, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		public OAuthTokenResource(IUserSource? UserSource)
			: this(UserSource, null, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthTokenResource(IUserSource? UserSource, JwtFactory? JwtFactory)
			: this(UserSource, JwtFactory, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthTokenResource(IUserSource? UserSource, string ResourceName)
			: this(UserSource, null, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthTokenResource(IUserSource? UserSource, JwtFactory? JwtFactory,
			string ResourceName)
			: base(ResourceName)
		{
			this.jwtFactory = JwtFactory;
			this.userSource = UserSource ?? Security.Users.Users.Source;
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
		/// Data source for users, used to authenticate clients.
		/// </summary>
		public IUserSource Users => this.userSource;

		internal async Task<string> GenerateTokenCode(IUserWithClaims User, bool Encrypted,
			string CodeChallenge, string CodeChallengeMethod, string RedirectUri)
		{
			if (this.jwtFactory is null)
				throw new ServiceUnavailableException("No JWT factory configured.");

			string Token = await User.CreateToken(this.jwtFactory, Encrypted);
			byte[] Bin = new byte[64];
			string Code;

			do
			{
				lock (rnd)
				{
					rnd.GetBytes(Bin);
				}

				Code = Base64Url.Encode(Bin);
			}
			while (tokenCache.ContainsKey(Code));

			tokenCache[Code] = new TokenRef(Token, User.UserName, CodeChallenge,
				CodeChallengeMethod, RedirectUri, 3600);

			return Code;
		}

		private class TokenRef
		{
			public TokenRef(string Token, string ClientId, string CodeChallenge,
				string CodeChallengeMethod, string RedirectUri, int ExpiresIn)
			{
				this.Token = Token;
				this.ClientId = ClientId;
				this.CodeChallenge = CodeChallenge;
				this.CodeChallengeMethod = CodeChallengeMethod;
				this.RedirectUri = RedirectUri;
				this.ExpiresIn = ExpiresIn;
			}

			public string Token;
			public string ClientId;
			public string CodeChallenge;
			public string CodeChallengeMethod;
			public string RedirectUri;
			public int ExpiresIn;

			public async Task<bool> Check(string CodeVerifier, HttpResponse Response)
			{
				switch (this.CodeChallengeMethod)
				{
					case "plain":
						if (CodeVerifier != this.CodeChallenge)
						{
							await Response.SendResponse(new ForbiddenException("Invalid code_verifier."));
							return false;
						}
						break;

					case "S256":
						string ExpectedCodeChallenge = Base64Url.Encode(
							Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(CodeVerifier)));

						if (ExpectedCodeChallenge != this.CodeChallenge)
						{
							await Response.SendResponse(new ForbiddenException("Invalid code_verifier."));
							return false;
						}
						break;

					default:
						await Response.SendResponse(new BadRequestException("Unsupported code_challenge_method: " + this.CodeChallengeMethod));
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
				await Response.SendResponse(new BadRequestException("Duplicate query parameters."));
				return;
			}

			if (!Request.Header.TryGetQueryParameter("code", out string Code))
			{
				await Response.SendResponse(new BadRequestException("Missing code."));
				return;
			}

			if (!tokenCache.TryGetValue(Code, out TokenRef Ref))
			{
				await Response.SendResponse(new ForbiddenException("Invalid code."));
				return;
			}

			if (!string.IsNullOrEmpty(Ref.CodeChallenge))
			{
				if (!Request.Header.TryGetQueryParameter("code_verifier", out string CodeVerifier))
				{
					await Response.SendResponse(new BadRequestException("Missing code_verifier."));
					return;
				}

				if (!await Ref.Check(CodeVerifier, Response))
					return;
			}

			tokenCache.Remove(Code);

			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");

			await Response.Return(TokenResponse(Ref.Token, null, Ref.ExpiresIn,
				string.Empty, this.jwtFactory?.Issuer));
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

			this.authenticationSchemes ??= CreateAuthenticationSchemes(this.jwtFactory, this.userSource);

			return this.authenticationSchemes;
		}

		internal static void GetDomainParameters(out string? Domain, out int MinStrength,
			out bool Encrypted)
		{
			if (!Types.TryGetModuleParameter("X509", out object Obj) ||
				!(Obj is X509Certificate Certificate))
			{
				if (Types.TryGetModuleParameter("Realm", out Obj) &&
					Obj is string Realm)
				{
					Domain = Realm;
				}
				else
					Domain = null;

				Encrypted = false;
				MinStrength = 0;
			}
			else
			{
				Encrypted = true;
				Domain = BinaryTcpClient.GetDomainFromSubject(Certificate.Subject);
				MinStrength = 128;
			}
		}

		internal static HttpAuthenticationScheme[] CreateAuthenticationSchemes(
			JwtFactory? JwtFactory, IUserSource Users)
		{
			// Note: Restricted set of authentication schemes, as compared to
			// HttpModule.GetAuthenticationSchemes().

			List<HttpAuthenticationScheme> Schemes = new List<HttpAuthenticationScheme>();

			GetDomainParameters(out string? Domain, out int MinStrength, out bool Encrypted);

			if (!(JwtFactory is null))
			{
				Schemes.Add(new JwtAuthentication(Encrypted, MinStrength, Domain, Users,
					JwtFactory));
			}

			HttpServer Server = Types.TryGetModuleParameter<HttpServer>("HTTP");

			if (!(Server is null) && Server.ClientCertificates != ClientCertificates.NotUsed)
				Schemes.Add(new MutualTlsAuthentication(Users));

			Schemes.Add(new BasicAuthentication(Encrypted, MinStrength, Domain, Users));
			Schemes.Add(new DigestAuthentication(Encrypted, MinStrength, DigestAlgorithm.MD5, Domain, Users));
			Schemes.Add(new DigestAuthentication(Encrypted, MinStrength, DigestAlgorithm.SHA256, Domain, Users));
			Schemes.Add(new DigestAuthentication(Encrypted, MinStrength, DigestAlgorithm.SHA3_256, Domain, Users));

			if (!(Server is null))
				Schemes.Add(new SessionAuthentication(Server));

			return Schemes.ToArray();
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			if (this.jwtFactory is null)
			{
				await Response.SendResponse(new ServiceUnavailableException("No JWT factory configured."));
				return;
			}

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException("No payload in request."));
				return;
			}

			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError || !(Content.Decoded is Dictionary<string, string> Form))
			{
				await Response.SendResponse(new BadRequestException("Expected URL-encoded WWW form."));
				return;
			}

			if (!Form.TryGetValue("grant_type", out string GrantType))
			{
				await Response.SendResponse(new BadRequestException("Missing grant_type."));
				return;
			}

			string Token;

			switch (GrantType)
			{
				case "authorization_code":
					string ClientId;

					if (!Form.TryGetValue("code", out string Code))
					{
						await Response.SendResponse(new BadRequestException());
						return;
					}

					if (!tokenCache.TryGetValue(Code, out TokenRef Ref))
					{
						await Response.SendResponse(new ForbiddenException("Invalid code."));
						return;
					}

					if (!Form.TryGetValue("redirect_uri", out string RedirectUri) ||
						!Form.TryGetValue("client_id", out ClientId))
					{
						await Response.SendResponse(new BadRequestException("Missing client_id."));
						return;
					}

					if (ClientId != Ref.ClientId)
					{
						await Response.SendResponse(new ForbiddenException());
						return;
					}

					if (Ref.RedirectUri != RedirectUri)
					{
						await Response.SendResponse(new ForbiddenException());
						return;
					}

					if (!string.IsNullOrEmpty(Ref.CodeChallenge))
					{
						if (!Form.TryGetValue("code_verifier", out string CodeVerifier))
						{
							await Response.SendResponse(new BadRequestException("Missing code_verifier."));
							return;
						}

						if (!await Ref.Check(CodeVerifier, Response))
							return;
					}

					tokenCache.Remove(Code);
					Token = Ref.Token;
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
								await Response.SendResponse(new BadRequestException());
								return;
							}

							if (!(Request.User is IUserWithClaims UserWithClaims))
							{
								await Response.SendResponse(ForbiddenException.AccessDenied(
									this.ResourceName, Request.RemoteEndPoint));
								return;
							}

							Token = await UserWithClaims.CreateToken(this.jwtFactory, Request.Encrypted);
							break;
						}
					}

					if (HasCredentials)
					{
						if (!Request.Encrypted && (Request.Server.OpenHttpsPorts?.Length ?? 0) > 0)
						{
							await Response.SendResponse(new BadRequestException(
								"Request must be performed over an encrypted connection."));
							return;
						}

						if (Request.Encrypted && Request.CipherStrength < 128)
						{
							await Response.SendResponse(new BadRequestException(
								"Cipher strength too weak."));
							return;
						}

						if (string.IsNullOrEmpty(this.realm))
							GetDomainParameters(out this.realm, out _, out _);

						LoginResult? LoginResult = await DoLogin(ClientId, ClientSecret,
							this.userSource, Request, this.realm ?? string.Empty);

						if (LoginResult is null)
						{
							await Response.SendResponse(new ForbiddenException(
								"User cannot authenticate via this interface."));
							return;
						}

						switch (LoginResult.Type)
						{
							case LoginResultType.Success:
								Request.User = LoginResult.User;
								break;

							case LoginResultType.InvalidCredentials:
							default:
								await Response.SendResponse(new ForbiddenException(
									"Invalid client_id or client_secret."));
								return;

							case LoginResultType.NoPassword:
								await Response.SendResponse(new ForbiddenException(
									"No or empty client_secret."));
								return;

							case LoginResultType.TemporarilyBlocked:
								await Response.SendResponse(new ForbiddenException(
									"Temporarily blocked. Try again after: " +
									LoginResult.Next?.ToString()));
								return;

							case LoginResultType.PermanentlyBlocked:
								await Response.SendResponse(new ForbiddenException(
									"Permanently blocked."));
								return;
						}

						if (!(Request.User is IUserWithClaims UserWithClaims))
						{
							await Response.SendResponse(ForbiddenException.AccessDenied(
								this.ResourceName, Request.RemoteEndPoint));
							return;
						}

						Token = await UserWithClaims.CreateToken(this.jwtFactory, Request.Encrypted);
					}
					else
					{
						await Response.SendResponse(new BadRequestException());
						return;
					}
					break;

				default:
					await Response.SendResponse(new BadRequestException("Unsupported grant_type: " + GrantType));
					return;
			}

			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");

			await Response.Return(TokenResponse(Token, null, 3600, string.Empty, 
				this.jwtFactory?.Issuer));
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
				return new LoginResult((IUser?)null);
			}
		}

		internal static Dictionary<string, object> TokenResponse(string Token, 
			string? State, int ExpiresIn, string Scope, string? Issuer)
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

			return Result;
		}
	}
}
