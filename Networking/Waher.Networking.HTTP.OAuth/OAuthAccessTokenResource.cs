using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.HTTP.Authentication;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Security.JWT;
using Waher.Security.Users;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH access_token resource.
	/// </summary>
	public class OAuthAccessTokenResource : HttpSynchronousResource, IHttpPostMethod
	{
		private HttpAuthenticationScheme[]? authenticationSchemes = null;
		private JwtFactory? jwtFactory;

		/// <summary>
		/// OAUTH access_token resource.
		/// </summary>
		public OAuthAccessTokenResource()
			: this(null, "/oauth/access_token")
		{
		}

		/// <summary>
		/// OAUTH access_token resource.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthAccessTokenResource(JwtFactory? JwtFactory)
			: this(JwtFactory, "/oauth/access_token")
		{
		}

		/// <summary>
		/// OAUTH access_token resource.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthAccessTokenResource(string ResourceName)
			: this(null, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH access_token resource.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthAccessTokenResource(JwtFactory? JwtFactory, string ResourceName)
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

			if (this.authenticationSchemes is null)
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

				if (!(this.jwtFactory is null))
				{
					Schemes.Add(new JwtAuthentication(Encrypted, MinStrength, Domain, null,
						this.jwtFactory));
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

				this.authenticationSchemes = Schemes.ToArray();
			}

			return this.authenticationSchemes;
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

			if (Request.User is null)
			{
				switch (GrantType)
				{
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
			}

			if (!(Request.User is IUserWithClaims UserWithClaims))
			{
				await Response.SendResponse(ForbiddenException.AccessDenied(
					this.ResourceName, Request.RemoteEndPoint));
				return;
			}

			string Token = await UserWithClaims.CreateToken(this.jwtFactory, Request.Encrypted);

			// Some clients (for example Postman) seem to have trouble decoding
			// transfer-encoded content.
			Response.EnableDirectTransfer();

			await Response.Return(new Dictionary<string, object>()
			{
				{ "access_token", Token },
				{ "token_type", "Bearer" },
				{ "expires_in", 3600 },
				{ "scope", string.Empty }
			});
		}

	}
}
