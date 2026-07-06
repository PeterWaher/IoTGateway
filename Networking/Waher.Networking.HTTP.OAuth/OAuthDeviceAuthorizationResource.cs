using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Content.Xml;
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
			if (!Request.Header.TryGetQueryParameter("user_code", out string UserCode))
				UserCode = string.Empty;

			await Response.Return(await this.GenerateAuthorizationForm(Request, Response,
				UserCode, string.Empty, false, false, string.Empty));
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
					UserCode = ComputeUserCode(DeviceCode, ClientId, Owner);
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
			else if (Form.TryGetValue("user_code", out string UserCode))
			{
				if (!Form.TryGetValue("UserName", out string UserName))
					UserName = string.Empty;

				if (!Form.TryGetValue("Password", out string Password))
					Password = string.Empty;

				if (!Form.TryGetValue("Accept", out string s) ||
					!CommonTypes.TryParse(s, out bool Accept))
				{
					Accept = false;
				}

				if (!Form.TryGetValue("Decline", out s) ||
					!CommonTypes.TryParse(s, out bool Decline))
				{
					Decline = false;
				}

				IUser? Device;
				IUser? Owner;

				if (string.IsNullOrEmpty(UserCode))
					s = "Missing User Code";
				else if (!codes.TryGetValue(UserCode, out DeviceRef DeviceRef))
					s = "Invalid User Code";
				else if (string.IsNullOrEmpty(UserName))
					s = "Missing user name.";
				else if (string.IsNullOrEmpty(Password))
					s = "Missing password.";
				else if (!Accept || !Decline)
					s = "You must either accept or decline the authorization request.";
				else if (Accept && Decline)
					s = "You cannot both accept and decline the authorization request.";
				else if ((Device = await this.Users.TryGetUser(DeviceRef.ClientId)) is null)
					s = "Device no longer registered.";
				else if ((Owner = await ThingRegistry.TryGetOwner(Device)) is null)
					s = "Device no longer has owner registered.";
				else if (Owner.UserName != UserName || UserCode != ComputeUserCode(
					DeviceRef.DeviceCode, DeviceRef.ClientId, Owner))
				{
					s = "Invalid user name, password, or owner.";
				}
				else
				{
					DeviceRef.Result = Accept;
					await Response.Return(await this.GenerateResult(Request, Response, Accept));
					return;
				}

				await Response.Return(await this.GenerateAuthorizationForm(Request, Response,
					UserCode, UserName, Accept, Decline, s));
			}
			else
				await BadRequest(Response, "invalid_request", "Missing client_id or user_code.");
		}

		private static string ComputeUserCode(string DeviceCode, string ClientId, IUser Owner)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(DeviceCode);
			sb.Append('|');
			sb.Append(ClientId);
			sb.Append('|');
			sb.Append(Owner.UserName);
			sb.Append('|');
			sb.Append(Owner.PasswordHash);

			SHAKE256 H = new SHAKE256(64);
			return Base64Url.Encode(H.ComputeVariable(Encoding.UTF8.GetBytes(sb.ToString())));
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
			public bool? Result;
		}

		private async Task<HtmlDocument> GenerateAuthorizationForm(HttpRequest Request,
			HttpResponse Response, string UserCode, string UserName, bool Accept,
			bool Decline, string ErrorMessage)
		{
			StringBuilder Markdown = new StringBuilder();

			Markdown.AppendLine("Title: Device Authorization");
			Markdown.AppendLine("Description: OAUTH device authorization page.");

			if (Request.Server.TryGetLocalResourceFileName("/Master.md", Request.Host, out string FileName) &&
				File.Exists(FileName))
			{
				Markdown.AppendLine("Master: /Master.md");
			}

			Markdown.Append("Date: ");
			Markdown.AppendLine(CommonTypes.EncodeRfc822(DateTime.UtcNow));
			Markdown.AppendLine();
			Markdown.AppendLine(new string('=', 40));
			Markdown.AppendLine();

			Markdown.AppendLine("Device Authorization");
			Markdown.AppendLine("=======================");
			Markdown.AppendLine();

			Markdown.Append("<form id='AuthorizationForm' action='");
			Markdown.Append(this.ResourceName);
			Markdown.Append("' method='post'>");
			Markdown.AppendLine();

			Markdown.AppendLine("<p>");
			Markdown.AppendLine("<label for='user_code'>User Code:</label>  ");
			Markdown.Append("<input name='user_code' type='text' autofocus autocomplete='off");

			if (!string.IsNullOrEmpty(UserCode))
			{
				Markdown.Append("' value='");
				Markdown.Append(XML.HtmlAttributeEncode(UserCode));
			}

			Markdown.AppendLine("'/>");
			Markdown.AppendLine("</p>");
			Markdown.AppendLine();

			Markdown.AppendLine("<p>");
			Markdown.Append("<input name='Accept' type='checkbox' ");
			Markdown.Append("title='Check this box to authorize the device access.'");
			if (Accept)
				Markdown.Append(" checked");
			Markdown.AppendLine("/>");
			Markdown.AppendLine("<label for='Accept'>Accept authorization.</label>  ");
			Markdown.AppendLine("</p>");
			Markdown.AppendLine();

			Markdown.AppendLine("<p>");
			Markdown.Append("<input name='Decline' type='checkbox' ");
			Markdown.Append("title='Check this box to decline the authorization request.'");
			if (Decline)
				Markdown.Append(" checked");
			Markdown.AppendLine("/>");
			Markdown.AppendLine("<label for='Decline'>Decline authorization.</label>  ");
			Markdown.AppendLine("</p>");
			Markdown.AppendLine();

			Markdown.AppendLine("<p>");
			Markdown.AppendLine("<label for='UserName'>User Name:</label>  ");
			Markdown.Append("<input name='UserName' type='text' autocomplete='username");
			if (!string.IsNullOrEmpty(UserName))
			{
				Markdown.Append("' value='");
				Markdown.Append(XML.HtmlAttributeEncode(UserName));
			}
			Markdown.AppendLine("'/>");
			Markdown.AppendLine("</p>");
			Markdown.AppendLine();

			Markdown.AppendLine("<p>");
			Markdown.AppendLine("<label for='Password'>Password:</label>  ");
			Markdown.Append("<input name='Password' type='password' ");
			Markdown.AppendLine("autocomplete='current-password' autofocus/>");
			Markdown.AppendLine("</p>");
			Markdown.AppendLine();

			if (!string.IsNullOrEmpty(ErrorMessage))
			{
				Markdown.AppendLine("<p>");
				Markdown.Append("<strong id='errorMessage'>");
				Markdown.Append(XML.HtmlValueEncode(ErrorMessage));
				Markdown.AppendLine("</strong>");
				Markdown.AppendLine("</p>");
				Markdown.AppendLine();
			}

			Markdown.AppendLine("<button type='submit'>Submit</button>");
			Markdown.AppendLine("</form>");

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown.ToString());
			string Html = await Doc.GenerateHTML();

			Response.SetHeader("X-Frame-Options", "DENY");
			Response.SetHeader("Content-Security-Policy", "frame-ancestors 'none'; default-src 'self'; script-src 'self'; object-src 'none'; base-uri 'none'; form-action 'self'");

			return new HtmlDocument(Html);
		}

		private async Task<HtmlDocument> GenerateResult(HttpRequest Request,
			HttpResponse Response, bool Accepted)
		{
			StringBuilder Markdown = new StringBuilder();

			Markdown.AppendLine("Title: Accepted");
			Markdown.AppendLine("Description: OAUTH device authorization has been accepted.");

			if (Request.Server.TryGetLocalResourceFileName("/Master.md", Request.Host, out string FileName) &&
				File.Exists(FileName))
			{
				Markdown.AppendLine("Master: /Master.md");
			}

			Markdown.Append("Date: ");
			Markdown.AppendLine(CommonTypes.EncodeRfc822(DateTime.UtcNow));
			Markdown.AppendLine();
			Markdown.AppendLine(new string('=', 40));
			Markdown.AppendLine();

			if (Accepted)
				Markdown.AppendLine("Accepted");
			else
				Markdown.AppendLine("Declined");

			Markdown.AppendLine("===========");
			Markdown.AppendLine();

			Markdown.Append("Authorization request has been ");

			if (Accepted)
			{
				Markdown.AppendLine("accepted.");
				Markdown.AppendLine("The device will be informed and granted access.");
			}
			else
			{
				Markdown.AppendLine("declined.");
				Markdown.AppendLine("The device will be informed access has been denied.");
			}

			Markdown.AppendLine("You can safely close this tab.");

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown.ToString());
			string Html = await Doc.GenerateHTML();

			Response.SetHeader("X-Frame-Options", "DENY");
			Response.SetHeader("Content-Security-Policy", "frame-ancestors 'none'; default-src 'self'; script-src 'self'; object-src 'none'; base-uri 'none'; form-action 'self'");

			return new HtmlDocument(Html);
		}

	}
}
