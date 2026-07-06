using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP.OAuth.Interfaces;
using Waher.Runtime.Cache;
using Waher.Security;
using Waher.Security.JWT;
using Waher.Security.SHA3;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// OAUTH device authorization resource, as defined in RFC 8628.
	/// https://datatracker.ietf.org/doc/html/rfc8628
	/// </summary>
	public class OAuthDeviceAuthorizationResource : OAuthResource, IHttpGetMethod,
		IHttpPostMethod
	{
		/// <summary>
		/// Default token resource path: /oauth/device
		/// </summary>
		public const string DefaultResourcePath = "/oauth/device";

		/// <summary>
		/// Grant Type for device authorization flow.
		/// </summary>
		public const string GrantType = "urn:ietf:params:oauth:grant-type:device_code";

		private static readonly Cache<string, DeviceRef> codes = new Cache<string, DeviceRef>(int.MaxValue, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

		/// <summary>
		/// OAUTH device authorization resource, as defined in RFC 8628.
		/// </summary>
		public OAuthDeviceAuthorizationResource()
			: this(null, null, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource, as defined in RFC 8628.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthDeviceAuthorizationResource(JwtFactory? JwtFactory)
			: this(null, JwtFactory, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource, as defined in RFC 8628.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthDeviceAuthorizationResource(string ResourceName)
			: this(null, null, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource, as defined in RFC 8628.
		/// </summary>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthDeviceAuthorizationResource(JwtFactory? JwtFactory, string ResourceName)
			: this(null, JwtFactory, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource, as defined in RFC 8628.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		public OAuthDeviceAuthorizationResource(IUserSource? UserSource)
			: this(UserSource, null, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource, as defined in RFC 8628.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		public OAuthDeviceAuthorizationResource(IUserSource? UserSource, JwtFactory? JwtFactory)
			: this(UserSource, JwtFactory, DefaultResourcePath)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource, as defined in RFC 8628.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthDeviceAuthorizationResource(IUserSource? UserSource, string ResourceName)
			: this(UserSource, null, ResourceName)
		{
		}

		/// <summary>
		/// OAUTH device authorization resource, as defined in RFC 8628.
		/// </summary>
		/// <param name="UserSource">Users data source.</param>
		/// <param name="JwtFactory">JWT Factory</param>
		/// <param name="ResourceName">Resource name.</param>
		public OAuthDeviceAuthorizationResource(IUserSource? UserSource, JwtFactory? JwtFactory,
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

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
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

			if (!(this.Users is IThingRegistryUserSource ThingRegistry))
			{
				await ServiceUnavailable(Response, "server_error",
					"Device authorization service not available.");
				return;
			}

			if (Form.TryGetValue("client_id", out string ClientId))
			{
				if (string.IsNullOrEmpty(ClientId))
				{
					await BadRequest(Response, "invalid_request", "Empty client_id.");
					return;
				}

				IUser? Device = await ThingRegistry.TryGetUser(ClientId);
				if (Device is null)
				{
					await ServiceUnavailable(Response, "access_denied", "Device or owner not registered.");
					return;
				}

				IUser? Owner = await ThingRegistry.TryGetOwner(Device);
				if (Owner is null)
				{
					await ServiceUnavailable(Response, "access_denied", "Device or owner not registered.");
					return;
				}

				string[] Scopes;

				if (Form.TryGetValue("scope", out string Scope))
					Scopes = Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				else
					Scopes = Array.Empty<string>();

				StringBuilder sb;
				string DeviceCode;
				string UserCode;

				do
				{
					DeviceCode = this.GenerateRandomCode(32);

					sb = new StringBuilder();
					sb.Append(DeviceCode);
					sb.Append('|');
					sb.Append(ClientId);
					sb.Append('|');
					sb.Append(Owner.UserName);
					sb.Append('|');
					sb.Append(Owner.PasswordHash);

					SHAKE256 H = new SHAKE256(128);
					UserCode = Base64Url.Encode(H.ComputeVariable(
						Encoding.UTF8.GetBytes(sb.ToString())));
				}
				while (codes.ContainsKey(UserCode));

				DeviceRef Ref = new DeviceRef(ClientId, Scopes, DeviceCode, UserCode);
				codes.Add(UserCode, Ref);

				Response.SetHeader("Cache-Control", "max-age=0, no-cache, no-store");
				Response.SetHeader("Pragma", "no-cache");

				sb = ProtectedResourceMetaData.GenerateServerUrl(Request, out _);
				sb.Append(this.ResourceName);
				string VerificationUrl = sb.ToString();

				sb.Append("?user_code=");
				sb.Append(UserCode);

				string VerificationUrlComplete = sb.ToString();

				await Response.Return(new Dictionary<string, object>()
				{
					{ "device_code", DeviceCode },
					{ "user_code", UserCode },
					{ "verification_uri", VerificationUrl },
					{ "verification_uri_complete", VerificationUrlComplete },
					{ "expires_in", 3600 },
					{ "interval", 5 }
				});
			}
			else
				await BadRequest(Response, "invalid_request", "Missing client_id.");
		}

		private class DeviceRef
		{
			public DeviceRef(string ClientId, string[] Scopes, string DeviceCode,
				string UserCode)
			{
				this.ClientId = ClientId;
				this.Scopes = Scopes;
				this.DeviceCode = DeviceCode;
				this.UserCode = UserCode;
			}

			public string ClientId;
			public string[] Scopes;
			public string DeviceCode;
			public string UserCode;
		}

	}
}
