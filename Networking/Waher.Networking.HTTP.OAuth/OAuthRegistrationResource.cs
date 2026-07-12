using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP.OAuth.Interfaces;
using Waher.Runtime.Collections;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH dynamic registration resource, as defined in RFCs 7591 and 7592.
	/// https://datatracker.ietf.org/doc/html/rfc7591
	/// https://datatracker.ietf.org/doc/html/rfc7592
	/// </summary>
	public class OAuthRegistrationResource : OAuthResource, IHttpPostMethod
	{
		/// <summary>
		/// Default registration resource path: /oauth/register
		/// </summary>
		public const string DefaultResourcePath = "/oauth/register";

		/// <summary>
		/// OAUTH dynamic registration resource, as defined in RFCs 7591 and 7592.
		/// </summary>
		/// <param name="Environment">OAuth2 environment.</param>
		public OAuthRegistrationResource(OAuth2Environment Environment)
			: this(Environment, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH dynamic registration resource, as defined in RFCs 7591 and 7592.
		/// </summary>
		/// <param name="Environment">OAuth2 environment.</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthRegistrationResource(OAuth2Environment Environment,
			string ResourceName)
			: base(Environment, ResourceName)
		{
			Environment.Register(this);
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

			ContentResponse Decoded = await Request.DecodeDataAsync();
			if (Decoded.HasError ||
				!(Decoded.Decoded is Dictionary<string, object> RequestObj))
			{
				await BadRequest(Response, "invalid_request", "Invalid form.");
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
			bool ReturnClientSecret = false;

			foreach (KeyValuePair<string, object> P in RequestObj)
			{
				switch (P.Key)
				{
					case "redirect_uris":
						RedirectUris = ToStrings(P.Value);

						if (!(RedirectUris is null))
						{
							foreach (string RedirectUri in RedirectUris)
							{
								if (!Uri.TryCreate(RedirectUri, UriKind.Absolute, out Uri Parsed) ||
									!string.IsNullOrEmpty(Parsed.Fragment) ||
									!string.IsNullOrEmpty(Parsed.Query))
								{
									await BadRequest(Response, "invalid_redirect_uri", "Invalid redirection URI.");
									return;
								}
							}
						}
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

						switch (TokenEndpointAuthMethod)
						{
							case "client_secret_post":
							case "client_secret_basic":
								ReturnClientSecret = true;
								break;
						}
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
							await BadRequest(Response, "invalid_request", "Invalid client_uri");
							return;
						}
						break;

					case "logo_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out LogoUri))
						{
							await BadRequest(Response, "invalid_request", "Invalid logo_uri");
							return;
						}
						break;

					case "tos_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out TosUri))
						{
							await BadRequest(Response, "invalid_request", "Invalid tos_uri");
							return;
						}
						break;

					case "policy_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out PolicyUri))
						{
							await BadRequest(Response, "invalid_request", "Invalid policy_uri");
							return;
						}
						break;

					case "jwks_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out JwksUri))
						{
							await BadRequest(Response, "invalid_request", "Invalid jwks_uri");
							return;
						}
						break;

					case "scope":
						string Scope = P.Value?.ToString() ?? string.Empty;
						if (!IsValidScope(Scope))
						{
							await BadRequest(Response, "invalid_scope", "Invalid scope parameter.");
							return;
						}

						Scopes = Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
						break;

					case "jwks":
						if (P.Value is Dictionary<string, object?> Jwks2)
							Jwks = Jwks2;
						else
						{
							await BadRequest(Response, "invalid_request", "Invalid jwks");
							return;
						}
						break;

					case "client_id":
					case "client_secret":
					case "client_id_issued_at":
					case "client_secret_expires_at":
						await BadRequest(Response, "invalid_request", "Invalid request parameter: " + P.Key);
						return;

					default:
						MetaData ??= new Dictionary<string, object?>();
						MetaData[P.Key] = P.Value;
						break;
				}
			}

			if (!(GrantTypes is null) &&
				Array.IndexOf(GrantTypes, "implicit") >= 0 &&
				!(ResponseTypes is null) &&
				Array.IndexOf(ResponseTypes, "token") < 0)
			{
				await BadRequest(Response, "invalid_client_metadata",
					"Implicit grant_type requires token response_type.");
				return;
			}

			if (!(this.Users is IDynamicUserSource DynamicUserSource))
			{
				await ServiceUnavailable(Response, "server_error",
					"Client registration service not available.");
				return;
			}

			IRegistration? Registration = await DynamicUserSource.RegisterUser(
				new RegistrationRequest(Request.RemoteEndPoint, RedirectUris,
				GrantTypes, ResponseTypes, TokenEndpointAuthMethod, ClientName, SoftwareId,
				SoftwareVersion, ClientUri, LogoUri, TosUri, PolicyUri, JwksUri, Scopes,
				Contacts, Jwks, MetaData));

			if (Registration is null)
			{
				await Forbidden(Response, "access_denied", 
					"Not permitted to register new client.");
				return;
			}


			Dictionary<string, object> ResponseObj = new Dictionary<string, object>();

			foreach (KeyValuePair<string, object> P in RequestObj)
				ResponseObj[P.Key] = P.Value;

			ResponseObj["client_id"] = Registration.ClientId;
			ResponseObj["client_id_issued_at"] = (long)DateTime.UtcNow.Subtract(JSON.UnixEpoch).TotalSeconds;

			if (ReturnClientSecret)
			{
				ResponseObj["client_secret"] = Registration.ClientSecret;
				ResponseObj["client_secret_expires_at"] = Registration.ClientSecretExpiresAt.HasValue ?
						(long)Registration.ClientSecretExpiresAt.Value.Subtract(JSON.UnixEpoch).TotalSeconds : 0L;
			}

			Response.StatusCode = 201;
			Response.StatusMessage = "Created";
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
