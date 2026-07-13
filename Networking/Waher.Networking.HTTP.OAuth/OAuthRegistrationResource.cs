using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP.OAuth.Clients;
using Waher.Networking.HTTP.OAuth.Interfaces;
using Waher.Persistence;
using Waher.Persistence.Filters;
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

			ParsedRegistrationRequest? Parsed = await this.ParseRegistrationRequest(
				Request, Response, false);

			if (Parsed is null)
				return;

			RegistrationRequest RegistrationRequest = Parsed.Request;

			IRegistration? Registration = await Parsed.DynamicUserSource.RegisterUser(
				RegistrationRequest);

			if (Registration is null)
			{
				await Forbidden(Response, "access_denied",
					"Not permitted to register new client.");
				return;
			}

			DateTime TP = DateTime.UtcNow;
			OAuthClientInformation ClientInfo = new OAuthClientInformation()
			{
				ClientId = Registration.ClientId,
				ClientSecretExpiresAt = Registration.ClientSecretExpiresAt,
				AccessToken = this.Environment.GenerateRandomCode(64),
				Created = TP,
				Updated = TP,
				RemoteEndPoint = RegistrationRequest.RemoteEndPoint,
				RedirectUris = RegistrationRequest.RedirectUris,
				GrantTypes = RegistrationRequest.GrantTypes,
				ResponseTypes = RegistrationRequest.ResponseTypes,
				TokenEndpointAuthMethod = RegistrationRequest.TokenEndpointAuthMethod,
				ClientName = RegistrationRequest.ClientName,
				SoftwareId = RegistrationRequest.SoftwareId,
				SoftwareVersion = RegistrationRequest.SoftwareVersion,
				ClientUri = RegistrationRequest.ClientUri?.ToString(),
				LogoUri = RegistrationRequest.LogoUri?.ToString(),
				TosUri = RegistrationRequest.TosUri?.ToString(),
				PolicyUri = RegistrationRequest.PolicyUri?.ToString(),
				JwksUri = RegistrationRequest.JwksUri?.ToString(),
				Scopes = RegistrationRequest.Scopes,
				Contacts = RegistrationRequest.Contacts,
				Jwks = RegistrationRequest.Jwks,
				MetaData = RegistrationRequest.MetaData
			};

			await Database.Insert(ClientInfo);
			await AddRedirectUrls(Registration.ClientId, RegistrationRequest.RedirectUris);

			Dictionary<string, object> ResponseObj = this.RegistrationResponse(Request, 
				Response, Parsed, ClientInfo, Registration);

			Response.StatusCode = 201;
			Response.StatusMessage = "Created";

			await Response.Return(ResponseObj);
		}

		internal Dictionary<string, object> RegistrationResponse(HttpRequest Request, 
			HttpResponse Response, ParsedRegistrationRequest? Parsed, 
			OAuthClientInformation ClientInfo, IRegistration? Registration)
		{
			Dictionary<string, object> ResponseObj = new Dictionary<string, object>();

			if (!(Parsed is null))
			{
				foreach (KeyValuePair<string, object> P in Parsed.RequestObj)
					ResponseObj[P.Key] = P.Value;
			}

			ResponseObj["client_id"] = ClientInfo.ClientId!;
			ResponseObj["client_id_issued_at"] = (long)ClientInfo.Created.Subtract(JSON.UnixEpoch).TotalSeconds;

			if (this.Environment.HasManagementResource)
			{
				string RegistrationClientUri = Request.Header.GetURL(false, false).
					Replace(DefaultResourcePath, OAuthManagementResource.DefaultResourcePath) +
					"/" + ClientInfo.ObjectId;

				ResponseObj["registration_access_token"] = ClientInfo.AccessToken!;
				ResponseObj["registration_client_uri"] = RegistrationClientUri;
			}

			if (!(Registration is null) && (Parsed?.ReturnClientSecret ?? false))
				ResponseObj["client_secret"] = Registration.ClientSecret;

			if ((!(Registration is null) && (Parsed?.ReturnClientSecret ?? false)) ||
				ClientInfo.ClientSecretExpiresAt.HasValue)
			{
				ResponseObj["client_secret_expires_at"] = ClientInfo.ClientSecretExpiresAt.HasValue ?
					(long)ClientInfo.ClientSecretExpiresAt.Value.Subtract(JSON.UnixEpoch).TotalSeconds : 0L;
			}

			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");

			return ResponseObj;
		}

		internal static async Task AddRedirectUrls(string ClientId, string[]? RedirectUris)
		{
			if (!(RedirectUris is null))
			{
				foreach (string RedirectUri in RedirectUris)
				{
					OAuthRedirectUri? UriObj = new OAuthRedirectUri()
					{
						ClientId = ClientId,
						Uri = RedirectUri
					};

					await Database.Insert(UriObj);
				}
			}
		}

		internal async Task<ParsedRegistrationRequest?> ParseRegistrationRequest(HttpRequest Request,
			HttpResponse Response, bool PermitClientCredentials)
		{
			ContentResponse Decoded = await Request.DecodeDataAsync();
			if (Decoded.HasError ||
				!(Decoded.Decoded is Dictionary<string, object> RequestObj))
			{
				await BadRequest(Response, "invalid_request", "Invalid form.");
				return null;
			}

			string[]? RedirectUris = null;
			string[]? GrantTypes = null;
			string[]? ResponseTypes = null;
			string? TokenEndpointAuthMethod = null;
			string? ClientName = null;
			string? SoftwareId = null;
			string? SoftwareVersion = null;
			string? ClientId = null;
			string? ClientSecret = null;
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
									return null;
								}

								OAuthRedirectUri? UriObj = await Database.FindFirstIgnoreRest<OAuthRedirectUri>(
									new FilterFieldEqualTo("Uri", RedirectUri));

								if (!(UriObj is null))
								{
									if (ClientId is null &&
										RequestObj.TryGetValue("client_id", out object Obj) &&
										Obj is string ClientId3)
									{
										ClientId = ClientId3;
									}

									if (UriObj.ClientId != ClientId)
									{
										await BadRequest(Response, "invalid_client_metadata", "URI already registered.");
										return null;
									}
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
							return null;
						}
						break;

					case "logo_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out LogoUri))
						{
							await BadRequest(Response, "invalid_request", "Invalid logo_uri");
							return null;
						}
						break;

					case "tos_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out TosUri))
						{
							await BadRequest(Response, "invalid_request", "Invalid tos_uri");
							return null;
						}
						break;

					case "policy_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out PolicyUri))
						{
							await BadRequest(Response, "invalid_request", "Invalid policy_uri");
							return null;
						}
						break;

					case "jwks_uri":
						if (!Uri.TryCreate(P.Value?.ToString(), UriKind.Absolute, out JwksUri))
						{
							await BadRequest(Response, "invalid_request", "Invalid jwks_uri");
							return null;
						}
						break;

					case "scope":
						string Scope = P.Value?.ToString() ?? string.Empty;
						if (!IsValidScope(Scope))
						{
							await BadRequest(Response, "invalid_scope", "Invalid scope parameter.");
							return null;
						}

						Scopes = Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
						break;

					case "jwks":
						if (P.Value is Dictionary<string, object?> Jwks2)
							Jwks = Jwks2;
						else
						{
							await BadRequest(Response, "invalid_request", "Invalid jwks");
							return null;
						}
						break;

					case "client_id":
						if (!PermitClientCredentials || !(P.Value is string ClientId2))
						{
							await BadRequest(Response, "invalid_request", "Invalid request parameter: " + P.Key);
							return null;
						}

						ClientId = ClientId2;
						break;

					case "client_secret":
						if (!PermitClientCredentials || !(P.Value is string ClientSecret2))
						{
							await BadRequest(Response, "invalid_request", "Invalid request parameter: " + P.Key);
							return null;
						}

						ClientSecret = ClientSecret2;
						break;

					case "client_id_issued_at":
					case "client_secret_expires_at":
						await BadRequest(Response, "invalid_request", "Invalid request parameter: " + P.Key);
						return null;

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
				return null;
			}

			if (!(this.Users is IDynamicUserSource DynamicUserSource))
			{
				await ServiceUnavailable(Response, "server_error",
					"Client registration service not available.");
				return null;
			}

			RegistrationRequest RegistrationRequest = new RegistrationRequest(
				Request.RemoteEndPoint, RedirectUris, GrantTypes, ResponseTypes, 
				TokenEndpointAuthMethod, ClientName, SoftwareId, SoftwareVersion, 
				ClientUri, LogoUri, TosUri, PolicyUri, JwksUri, Scopes, Contacts, 
				Jwks, MetaData, ClientId, ClientSecret);

			return new ParsedRegistrationRequest(RegistrationRequest, RequestObj,
				DynamicUserSource, ReturnClientSecret);
		}

		internal class ParsedRegistrationRequest
		{
			public ParsedRegistrationRequest(RegistrationRequest Request,
				Dictionary<string, object> RequestObj, IDynamicUserSource DynamicUserSource, 
				bool ReturnClientSecret)
			{
				this.Request = Request;
				this.RequestObj = RequestObj;
				this.DynamicUserSource = DynamicUserSource;
				this.ReturnClientSecret = ReturnClientSecret;
			}

			public RegistrationRequest Request;
			public Dictionary<string, object> RequestObj;
			public IDynamicUserSource DynamicUserSource;
			public bool ReturnClientSecret;
		}

		internal class RegistrationRequest : IRegistrationRequest
		{
			public RegistrationRequest(string RemoteEndPoint, string[]? RedirectUris,
				string[]? GrantTypes, string[]? ResponseTypes,
				string? TokenEndpointAuthMethod, string? ClientName, string? SoftwareId,
				string? SoftwareVersion, Uri? ClientUri, Uri? LogoUri, Uri? TosUri,
				Uri? PolicyUri, Uri? JwksUri, string[]? Scopes, string[]? Contacts,
				Dictionary<string, object?>? Jwks, Dictionary<string, object?>? MetaData,
				string? ClientId, string? ClientSecret)
			{
				this.RemoteEndPoint = RemoteEndPoint;
				this.PublicClient = TokenEndpointAuthMethod == "none";
				this.ConfidentialClient = !this.PublicClient;
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
				this.ClientId = ClientId;
				this.ClientSecret = ClientSecret;
			}

			public string RemoteEndPoint { get; }
			public bool PublicClient { get; }
			public bool ConfidentialClient { get; }
			public string[]? RedirectUris { get; }
			public string[]? GrantTypes { get; }
			public string[]? ResponseTypes { get; }
			public string? TokenEndpointAuthMethod { get; }
			public string? ClientName { get; }
			public string? SoftwareId { get; }
			public string? SoftwareVersion { get; }
			public string? ClientId { get; }
			public string? ClientSecret { get; }
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
