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
using Waher.Security.Users;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH token resource.
	/// </summary>
	public class OAuthTokenResource : HttpSynchronousResource,
		IHttpGetMethod, IHttpPostMethod
	{
		private static readonly Cache<string, TokenRef> tokenCache = new Cache<string, TokenRef>(int.MaxValue, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
		private static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();
		private HttpAuthenticationScheme[]? authenticationSchemes = null;
		private JwtFactory? jwtFactory;

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		public OAuthTokenResource()
			: this(null, "/oauth/token")
		{
		}

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthTokenResource(JwtFactory? JwtFactory)
			: this(JwtFactory, "/oauth/token")
		{
		}

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthTokenResource(string ResourceName)
			: this(null, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH token resource.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthTokenResource(JwtFactory? JwtFactory, string ResourceName)
			: base(ResourceName)
		{
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

		internal async Task<string> GenerateTokenCode(IUserWithClaims User, bool Encrypted,
			string CodeChallenge, string CodeChallengeMethod)
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

			tokenCache[Code] = new TokenRef(Token, CodeChallenge, CodeChallengeMethod);

			return Code;
		}

		private class TokenRef
		{
			public TokenRef(string Token, string CodeChallenge, string CodeChallengeMethod)
			{
				this.Token = Token;
				this.CodeChallenge = CodeChallenge;
				this.CodeChallengeMethod = CodeChallengeMethod;
			}

			public string Token;
			public string CodeChallenge;
			public string CodeChallengeMethod;

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

			await Response.Return(new Dictionary<string, object>()
			{
				{ "access_token", Ref.Token },
				{ "token_type", "Bearer" },
				{ "expires_in", 3600 },
				{ "scope", string.Empty }
			});
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

			this.authenticationSchemes ??= CreateAuthenticationSchemes(this.jwtFactory);

			return this.authenticationSchemes;
		}

		internal static HttpAuthenticationScheme[] CreateAuthenticationSchemes(
			JwtFactory? JwtFactory)
		{
			// Note: Restricted set of authentication schemes, as compared to
			// HttpModule.GetAuthenticationSchemes().

			List<HttpAuthenticationScheme> Schemes = new List<HttpAuthenticationScheme>();
			string? Domain;
			int MinStrength;
			bool Encrypted;

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

			if (!(JwtFactory is null))
			{
				Schemes.Add(new JwtAuthentication(Encrypted, MinStrength, Domain, null,
					JwtFactory));
			}

			HttpServer Server = Types.TryGetModuleParameter<HttpServer>("HTTP");

			if (!(Server is null) && Server.ClientCertificates != ClientCertificates.NotUsed)
				Schemes.Add(new MutualTlsAuthentication(Users.Source));

			Schemes.Add(new BasicAuthentication(Encrypted, MinStrength, Domain, Users.Source));
			Schemes.Add(new DigestAuthentication(Encrypted, MinStrength, DigestAlgorithm.MD5, Domain, Users.Source));
			Schemes.Add(new DigestAuthentication(Encrypted, MinStrength, DigestAlgorithm.SHA256, Domain, Users.Source));
			Schemes.Add(new DigestAuthentication(Encrypted, MinStrength, DigestAlgorithm.SHA3_256, Domain, Users.Source));

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
					if (Form.TryGetValue("code", out string Code))
					{
						if (!tokenCache.TryGetValue(Code, out TokenRef Ref))
						{
							await Response.SendResponse(new ForbiddenException("Invalid code."));
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
					}
					else
					{
						await Response.SendResponse(ForbiddenException.AccessDenied(
							this.ResourceName, Request.RemoteEndPoint));
						return;
					}

				case "client_credentials":
				case "password":
					if (Form.TryGetValue("client_id", out string ClientId) &&
						Form.TryGetValue("client_secret", out string ClientSecret))
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

						string Nonce = Guid.NewGuid().ToString();
						byte[] PasswordHash = Users.ComputeHash(ClientId, ClientSecret);
						string PasswordNonceHash = Convert.ToBase64String(
							Hashes.ComputeHMACSHA256Hash(Encoding.UTF8.GetBytes(Nonce),
							PasswordHash));

						LoginResult LoginResult = await Users.Login(ClientId, PasswordNonceHash,
							Nonce, Request.RemoteEndPoint, "OAuth2");

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
						await Response.SendResponse(ForbiddenException.AccessDenied(
								this.ResourceName, Request.RemoteEndPoint));
						return;
					}
					break;

				default:
					await Response.SendResponse(new BadRequestException("Unsupported grant_type: " + GrantType));
					return;
			}

			await Response.Return(TokenResponse(Token));
		}

		internal static Dictionary<string, object> TokenResponse(string Token)
		{
			return new Dictionary<string, object>()
			{
				{ "access_token", Token },
				{ "token_type", "Bearer" },
				{ "expires_in", 3600 },
				{ "scope", string.Empty }
			};
		}
	}
}
