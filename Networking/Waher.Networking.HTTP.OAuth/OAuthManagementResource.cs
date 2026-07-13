using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP.OAuth.Clients;
using Waher.Networking.HTTP.OAuth.Interfaces;
using Waher.Persistence;
using Waher.Persistence.Filters;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH client management resource, as defined in RFCs 7591.
	/// https://datatracker.ietf.org/doc/html/rfc7591
	/// </summary>
	public class OAuthManagementResource : OAuthResource, IHttpGetMethod, IHttpPutMethod,
		IHttpDeleteMethod
	{
		/// <summary>
		/// OAUTH client management resource, as defined in RFCs 7591.
		/// </summary>
		public const string DefaultResourcePath = "/oauth/registration";

		/// <summary>
		/// OAUTH client management resource, as defined in RFCs 7591.
		/// </summary>
		/// <param name="Environment">OAuth2 environment.</param>
		public OAuthManagementResource(OAuth2Environment Environment)
			: this(Environment, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH client management resource, as defined in RFCs 7591.
		/// </summary>
		/// <param name="Environment">OAuth2 environment.</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthManagementResource(OAuth2Environment Environment,
			string ResourceName)
			: base(Environment, ResourceName)
		{
			Environment.Register(this);
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the PUT method is allowed.
		/// </summary>
		public bool AllowsPUT => true;

		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		public bool AllowsDELETE => true;

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		/// <returns>Array of authentication schemes (possibly empty) available for
		/// authenticating the user making the request. If no default authentication
		/// is to be performed, null can be returned.</returns>
		public override HttpAuthenticationScheme[]? GetAuthenticationSchemes(HttpRequest Request)
		{
			return null;
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			OAuthClientInformation? ClientInfo = await this.GetClientInformation(Request, Response);
			if (ClientInfo is null)
				return;

			Dictionary<string, object> ResponseObj = this.RegistrationResponse(Request, Response, ClientInfo);

			Response.StatusCode = 200;
			Response.StatusMessage = "OK";

			await Response.Return(ResponseObj);
		}

		internal Dictionary<string, object> RegistrationResponse(HttpRequest Request,
			HttpResponse Response, OAuthClientInformation ClientInfo)
		{
			Dictionary<string, object> ResponseObj = new Dictionary<string, object>
			{
				["client_id"] = ClientInfo.ClientId!,
				["client_id_issued_at"] = (long)ClientInfo.Created.Subtract(JSON.UnixEpoch).TotalSeconds
			};

			if (!(ClientInfo.RedirectUris is null))
				ResponseObj["redirect_uris"] = ClientInfo.RedirectUris;

			if (!(ClientInfo.GrantTypes is null))
				ResponseObj["grant_types"] = ClientInfo.GrantTypes;

			if (!(ClientInfo.ResponseTypes is null))
				ResponseObj["response_types"] = ClientInfo.ResponseTypes;

			if (!(ClientInfo.Contacts is null))
				ResponseObj["contacts"] = ClientInfo.Contacts;

			if (!string.IsNullOrEmpty(ClientInfo.TokenEndpointAuthMethod))
				ResponseObj["token_endpoint_auth_method"] = ClientInfo.TokenEndpointAuthMethod;

			if (!string.IsNullOrEmpty(ClientInfo.ClientName))
				ResponseObj["client_name"] = ClientInfo.ClientName;

			if (!string.IsNullOrEmpty(ClientInfo.SoftwareId))
				ResponseObj["software_id"] = ClientInfo.SoftwareId;

			if (!string.IsNullOrEmpty(ClientInfo.SoftwareVersion))
				ResponseObj["software_version"] = ClientInfo.SoftwareVersion;

			if (!string.IsNullOrEmpty(ClientInfo.ClientUri))
				ResponseObj["client_uri"] = ClientInfo.ClientUri;

			if (!string.IsNullOrEmpty(ClientInfo.LogoUri))
				ResponseObj["logo_uri"] = ClientInfo.LogoUri;

			if (!string.IsNullOrEmpty(ClientInfo.TosUri))
				ResponseObj["tos_uri"] = ClientInfo.TosUri;

			if (!string.IsNullOrEmpty(ClientInfo.PolicyUri))
				ResponseObj["policy_uri"] = ClientInfo.PolicyUri;

			if (!string.IsNullOrEmpty(ClientInfo.JwksUri))
				ResponseObj["jwks_uri"] = ClientInfo.JwksUri;

			if (!(ClientInfo.Jwks is null))
				ResponseObj["jwks"] = ClientInfo.Jwks;

			if ((ClientInfo.Scopes?.Length ?? 0) > 0)
				ResponseObj["scope"] = string.Join(' ', ClientInfo.Scopes);

			if (ClientInfo.ClientSecretExpiresAt.HasValue)
			{
				ResponseObj["client_secret_expires_at"] = ClientInfo.ClientSecretExpiresAt.HasValue ?
					(long)ClientInfo.ClientSecretExpiresAt.Value.Subtract(JSON.UnixEpoch).TotalSeconds : 0L;
			}

			ResponseObj["registration_access_token"] = ClientInfo.AccessToken!;
			ResponseObj["registration_client_uri"] = Request.Header.GetURL(false, false);

			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");

			return ResponseObj;
		}

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task PUT(HttpRequest Request, HttpResponse Response)
		{
			if (!this.Environment.HasRegistrationResource)
			{
				await ServiceUnavailable(Response, "server_error", "Registration resource not available.");
				return;
			}

			OAuthClientInformation? ClientInfo = await this.GetClientInformation(Request, Response);
			if (ClientInfo is null)
				return;

			OAuthRegistrationResource.ParsedRegistrationRequest? Parsed =
				await this.Environment.RegistrationResource.ParseRegistrationRequest(
					Request, Response, true);

			if (Parsed is null)
				return;

			OAuthRegistrationResource.RegistrationRequest RegistrationRequest = Parsed.Request;

			if (RegistrationRequest.ClientId != ClientInfo.ClientId)
			{
				await BadRequest(Response, "invalid_request", "Invalid registration or access token.");
				return;
			}

			IRegistration? Registration = await Parsed.DynamicUserSource.UpdateUser(
				ClientInfo.ClientId!, RegistrationRequest);

			if (Registration is null || ClientInfo.ClientId != Registration.ClientId)
			{
				await Forbidden(Response, "access_denied",
					"Not permitted to update client.");
				return;
			}

			bool UpdateRedirectUris = !AreEqual(ClientInfo.RedirectUris, RegistrationRequest.RedirectUris);

			ClientInfo.Updated = DateTime.UtcNow;
			ClientInfo.RemoteEndPoint = RegistrationRequest.RemoteEndPoint;
			ClientInfo.RedirectUris = RegistrationRequest.RedirectUris;
			ClientInfo.GrantTypes = RegistrationRequest.GrantTypes;
			ClientInfo.ResponseTypes = RegistrationRequest.ResponseTypes;
			ClientInfo.TokenEndpointAuthMethod = RegistrationRequest.TokenEndpointAuthMethod;
			ClientInfo.ClientName = RegistrationRequest.ClientName;
			ClientInfo.SoftwareId = RegistrationRequest.SoftwareId;
			ClientInfo.SoftwareVersion = RegistrationRequest.SoftwareVersion;
			ClientInfo.ClientUri = RegistrationRequest.ClientUri?.ToString();
			ClientInfo.LogoUri = RegistrationRequest.LogoUri?.ToString();
			ClientInfo.TosUri = RegistrationRequest.TosUri?.ToString();
			ClientInfo.PolicyUri = RegistrationRequest.PolicyUri?.ToString();
			ClientInfo.JwksUri = RegistrationRequest.JwksUri?.ToString();
			ClientInfo.Scopes = RegistrationRequest.Scopes;
			ClientInfo.Contacts = RegistrationRequest.Contacts;
			ClientInfo.Jwks = RegistrationRequest.Jwks;
			ClientInfo.MetaData = RegistrationRequest.MetaData;

			await Database.Update(ClientInfo);

			if (UpdateRedirectUris)
			{
				await Database.Delete<OAuthRedirectUri>(new FilterFieldEqualTo("ClientId",
					ClientInfo.ClientId));
				await OAuthRegistrationResource.AddRedirectUrls(Registration.ClientId,
					RegistrationRequest.RedirectUris);
			}

			Dictionary<string, object> ResponseObj = this.RegistrationResponse(Request,
				Response, ClientInfo);

			Response.StatusCode = 200;
			Response.StatusMessage = "OK";

			await Response.Return(ResponseObj);
		}

		private static bool AreEqual(string[]? A1, string[]? A2)
		{
			if (A1 is null ^ A2 is null)
				return false;

			if (A1 is null)
				return true;

			int i, c = A1.Length;
			if (A2!.Length != c)
				return false;

			HashSet<string> Set1 = new HashSet<string>(A1);
			HashSet<string> Set2 = new HashSet<string>(A2);

			if (Set1.Count != Set2.Count)
				return false;

			for (i = 0; i < c; i++)
			{
				if (!Set1.Contains(A2[i]))
					return false;
			}

			return true;
		}

		private async Task<OAuthClientInformation?> GetClientInformation(HttpRequest Request,
			HttpResponse Response)
		{
			string ClientId = Request.SubPath;
			if (string.IsNullOrEmpty(ClientId))
			{
				await BadRequest(Response, "invalid_request", "Invalid client URI.");
				return null;
			}

			string? Authorization = Request.Header.Authorization?.Value;
			if (string.IsNullOrEmpty(Authorization) || !Authorization.StartsWith("Bearer "))
			{
				await Unauthorized(Response, "access_denied", "Missing or invalid registration token.",
					Array.Empty<string>());
				return null;
			}

			Authorization = Authorization[7..].Trim();

			try
			{
				OAuthClientInformation Result = await Database.TryLoadObject<OAuthClientInformation>(
					ClientId[1..]);

				if (Result is null || Result.AccessToken != Authorization)
				{
					await Unauthorized(Response, "access_denied", "Missing or invalid registration token.",
						Array.Empty<string>());
					return null;
				}

				return Result;
			}
			catch (Exception)
			{
				await ServiceUnavailable(Response, "server_error",
					"Unable to retrieve client information.");
				return null;
			}
		}

		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task DELETE(HttpRequest Request, HttpResponse Response)
		{
			OAuthClientInformation? ClientInfo = await this.GetClientInformation(Request, Response);
			if (ClientInfo is null)
				return;

			if (!(this.Users is IDynamicUserSource DynamicUserSource))
			{
				await ServiceUnavailable(Response, "server_error", "Client registration service not available.");
				return;
			}

			if (ClientInfo.ClientId is null ||
				!await DynamicUserSource.DeleteUser(ClientInfo.ClientId, Request.RemoteEndPoint))
			{
				await Forbidden(Response, "access_denied",
					"Not permitted to delete client.");
				return;
			}

			FilterFieldEqualTo Filter = new FilterFieldEqualTo("ClientId", ClientInfo.ClientId);
			await Database.Delete<OAuthClientInformation>(Filter);
			await Database.Delete<OAuthRedirectUri>(Filter);

			Response.StatusCode = 204;
			Response.StatusMessage = "No Content";

			Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
			Response.SetHeader("Pragma", "no-cache");

			await Response.SendResponse();
		}
	}
}
