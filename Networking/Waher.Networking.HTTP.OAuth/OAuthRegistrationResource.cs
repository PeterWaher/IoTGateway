using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP.OAuth.Interfaces;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH registration resource.
	/// </summary>
	public class OAuthRegistrationResource : HttpSynchronousResource, IHttpPostMethod
	{
		/// <summary>
		/// Default registration resource path: /oauth/register
		/// </summary>
		public const string DefaultResourcePath = "/oauth/register";

		private readonly IDynamicUserSource userSource;
		private HttpAuthenticationScheme[]? authenticationSchemes = null;
		private JwtFactory? jwtFactory;

		/// <summary>
		/// OAUTH registration resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		public OAuthRegistrationResource(IDynamicUserSource UserSource)
			: this(UserSource, null, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH registration resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthRegistrationResource(IDynamicUserSource UserSource, JwtFactory? JwtFactory)
			: this(UserSource, JwtFactory, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH registration resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthRegistrationResource(IDynamicUserSource UserSource, string ResourceName)
			: this(UserSource, null, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH registration resource.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthRegistrationResource(IDynamicUserSource UserSource, JwtFactory? JwtFactory,
			string ResourceName)
			: base(ResourceName)
		{
			this.jwtFactory = JwtFactory;
			this.userSource = UserSource;
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
		/// Data source for users, used to authenticate clients.
		/// </summary>
		public IUserSource Users => this.userSource;

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

			this.authenticationSchemes ??= OAuthTokenResource.CreateAuthenticationSchemes(
				this.jwtFactory, this.userSource);

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
			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			ContentResponse Decoded = await Request.DecodeDataAsync();
			if (Decoded.HasError ||
				!(Decoded.Decoded is Dictionary<string, object> RequestObj))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			string[]? RedirectUris = null;
			string[]? GrantTypes = null;
			string[]? ResponseTypes = null;
			string? TokenEndpointAuthMethod = null;
			string? ClientName = null;
			string? SoftwareId = null;
			string? SoftwareVersion = null;
			Uri? ClientUri = null;
			Uri? LogoUri = null;
			Uri? TosUri = null;
			Uri? PolicyUri = null;
			Uri? JwksUri = null;
			string[]? Scopes = null;
			string[]? Contacts = null;
			Dictionary<string, object?>? Jwks = null;
			Dictionary<string, object?>? MetaData = null;

			foreach (KeyValuePair<string, object> P in RequestObj)
			{
				switch (P.Key)
				{
					case "redirect_uris":
						RedirectUris = ToStrings(P.Value);
						break;

					case "grant_types":
						GrantTypes = ToStrings(P.Value);
						break;

					case "response_types":
						ResponseTypes = ToStrings(P.Value);
						break;

					case "contacts":
						Contacts = ToStrings(P.Value);
						break;

					case "token_endpoint_auth_method":
						TokenEndpointAuthMethod = P.Value?.ToString();
						break;

					case "client_name":
						ClientName = P.Value?.ToString();
						break;

					case "software_id":
						SoftwareId = P.Value?.ToString();
						break;

					case "software_version":
						SoftwareVersion = P.Value?.ToString();
						break;

					case "client_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out ClientUri))
						{
							await Response.SendResponse(new BadRequestException("Invalid client_uri"));
							return;
						}
						break;

					case "logo_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out LogoUri))
						{
							await Response.SendResponse(new BadRequestException("Invalid logo_uri"));
							return;
						}
						break;

					case "tos_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out TosUri))
						{
							await Response.SendResponse(new BadRequestException("Invalid tos_uri"));
							return;
						}
						break;

					case "policy_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out PolicyUri))
						{
							await Response.SendResponse(new BadRequestException("Invalid policy_uri"));
							return;
						}
						break;

					case "jwks_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out JwksUri))
						{
							await Response.SendResponse(new BadRequestException("Invalid jwks_uri"));
							return;
						}
						break;

					case "scope":
						Scopes = P.Value?.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
						break;

					case "jwks":
						if (P.Value is Dictionary<string, object?> Jwks2)
							Jwks = Jwks2;
						else
						{
							await Response.SendResponse(new BadRequestException("Invalid jwks"));
							return;
						}
						break;

					case "client_id":
					case "client_secret":
					case "client_id_issued_at":
					case "client_secret_expires_at":
						await Response.SendResponse(new BadRequestException("Invalid request parameter: " + P.Key));
						return;

					default:
						MetaData ??= new Dictionary<string, object?>();
						MetaData[P.Key] = P.Value;
						break;
				}
			}

			IRegistration? Registration = await this.userSource.RegisterUser(
				new RegistrationRequest(Request.RemoteEndPoint, RedirectUris,
				GrantTypes, ResponseTypes, TokenEndpointAuthMethod, ClientName, SoftwareId,
				SoftwareVersion, ClientUri, LogoUri, TosUri, PolicyUri, JwksUri, Scopes,
				Contacts, Jwks, MetaData));

			if (Registration is null)
			{
				await Response.SendResponse(new ForbiddenException("Not permitted to register new client."));
				return;
			}


			Dictionary<string, object> ResponseObj = new Dictionary<string, object>();

			foreach (KeyValuePair<string, object> P in RequestObj)
				ResponseObj[P.Key] = P.Value;

			ResponseObj["client_id"] = Registration.ClientId;
			ResponseObj["client_secret"] = Registration.ClientSecret;
			ResponseObj["client_id_issued_at"] = (long)DateTime.UtcNow.Subtract(JSON.UnixEpoch).TotalSeconds;
			ResponseObj["client_secret_expires_at"] = Registration.ClientSecretExpiresAt.HasValue ?
					(long)Registration.ClientSecretExpiresAt.Value.Subtract(JSON.UnixEpoch).TotalSeconds : 0L;

			Response.StatusCode = 201;  // Created
			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");

			await Response.Return(ResponseObj);
		}

		private class RegistrationRequest : IRegistrationRequest
		{
			public RegistrationRequest(string RemoteEndPoint, string[]? RedirectUris,
				string[]? GrantTypes, string[]? ResponseTypes,
				string? TokenEndpointAuthMethod, string? ClientName, string? SoftwareId,
				string? SoftwareVersion, Uri? ClientUri, Uri? LogoUri, Uri? TosUri,
				Uri? PolicyUri, Uri? JwksUri, string[]? Scopes, string[]? Contacts,
				Dictionary<string, object?>? Jwks, Dictionary<string, object?>? MetaData)
			{
				this.RemoteEndPoint = RemoteEndPoint;
				this.RedirectUris = RedirectUris;
				this.GrantTypes = GrantTypes;
				this.ResponseTypes = ResponseTypes;
				this.TokenEndpointAuthMethod = TokenEndpointAuthMethod;
				this.ClientName = ClientName;
				this.SoftwareId = SoftwareId;
				this.SoftwareVersion = SoftwareVersion;
				this.ClientUri = ClientUri;
				this.LogoUri = LogoUri;
				this.TosUri = TosUri;
				this.PolicyUri = PolicyUri;
				this.JwksUri = JwksUri;
				this.Scopes = Scopes;
				this.Contacts = Contacts;
				this.Jwks = Jwks;
				this.MetaData = MetaData;
			}

			public string RemoteEndPoint { get; }
			public string[]? RedirectUris { get; }
			public string[]? GrantTypes { get; }
			public string[]? ResponseTypes { get; }
			public string? TokenEndpointAuthMethod { get; }
			public string? ClientName { get; }
			public string? SoftwareId { get; }
			public string? SoftwareVersion { get; }
			public Uri? ClientUri { get; }
			public Uri? LogoUri { get; }
			public Uri? TosUri { get; }
			public Uri? PolicyUri { get; }
			public Uri? JwksUri { get; }
			public string[]? Scopes { get; }
			public string[]? Contacts { get; }
			public Dictionary<string, object?>? Jwks { get; }
			public Dictionary<string, object?>? MetaData { get; }
		}

		private static string[]? ToStrings(object? Value)
		{
			if (Value is null)
				return null;
			else if (Value is string[] Strings)
				return Strings;
			else if (Value is IEnumerable Items)
			{
				ChunkedList<string> Result = new ChunkedList<string>();

				foreach (object Item in Items)
				{
					if (Item is string s)
						Result.Add(s);
					else
						Result.Add(Item.ToString());
				}

				return Result.ToArray();
			}
			else
				return new string[] { Value.ToString() };
		}
	}
}
