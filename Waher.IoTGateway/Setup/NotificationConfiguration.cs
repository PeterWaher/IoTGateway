using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Notification Configuration
	/// </summary>
	public class NotificationConfiguration : SystemConfiguration
	{
		private static NotificationConfiguration instance = null;
		private HttpResource testAddresses = null;

		private CaseInsensitiveString[] addresses = Array.Empty<CaseInsensitiveString>();
		private CaseInsensitiveString[] urls = Array.Empty<CaseInsensitiveString>();

		/// <summary>
		/// Notification Configuration
		/// </summary>
		public NotificationConfiguration()
			: base()
		{
		}

		/// <summary>
		/// Current instance of configuration.
		/// </summary>
		public static NotificationConfiguration Instance => instance;

		/// <summary>
		/// Notification addresses.
		/// </summary>
		[DefaultValueNull]
		public CaseInsensitiveString[] Addresses
		{
			get => this.addresses;
			set => this.addresses = value;
		}

		/// <summary>
		/// Notification addresses.
		/// </summary>
		[DefaultValueNull]
		public CaseInsensitiveString[] Urls
		{
			get => this.urls;
			set => this.urls = value;
		}

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public override string Resource => "/Settings/Notification.md";

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public override int Priority => 600;

		/// <summary>
		/// Gets a title for the system configuration.
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Title string</returns>
		public override Task<string> Title(Language Language)
		{
			return Language.GetStringAsync(typeof(Gateway), 2, "Notification");
		}

		/// <summary>
		/// Is called during startup to configure the system.
		/// </summary>
		public override Task ConfigureSystem()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Sets the static instance of the configuration.
		/// </summary>
		/// <param name="Configuration">Configuration object</param>
		public override void SetStaticInstance(ISystemConfiguration Configuration)
		{
			instance = Configuration as NotificationConfiguration;
		}

		/// <summary>
		/// Initializes the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task InitSetup(HttpServer WebServer)
		{
			this.testAddresses = WebServer.Register("/Settings/TestNotificationAddresses", null, this.TestNotificationAddresses, true, false, true);

			return base.InitSetup(WebServer);
		}

		/// <summary>
		/// Unregisters the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task UnregisterSetup(HttpServer WebServer)
		{
			WebServer.Unregister(this.testAddresses);

			return base.UnregisterSetup(WebServer);
		}

		/// <summary>
		/// Minimum required privilege for a user to be allowed to change the configuration defined by the class.
		/// </summary>
		protected override string ConfigPrivilege => "Admin.Communication.Notification";

		private async Task TestNotificationAddresses(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError ||
				!(Content.Decoded is Dictionary<string, object> Obj) ||
				!Obj.TryGetValue("NotificationAddresses", out object Obj2) ||
				!(Obj2 is string NotificationAddresses) ||
				!Obj.TryGetValue("NotificationUrls", out Obj2) ||
				!(Obj2 is string NotificationUrls))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			string TabID = Request.Header["X-TabID"];

			List<CaseInsensitiveString> Addresses = new List<CaseInsensitiveString>();
			List<CaseInsensitiveString> WebHooks = new List<CaseInsensitiveString>();

			Response.StatusCode = 200;

			try
			{
				foreach (string Part in NotificationAddresses.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries))
				{
					string s = Part.Trim();
					if (string.IsNullOrEmpty(s))
						continue;

					if (string.Compare(s, Gateway.XmppClient.BareJID, true) == 0)
						continue;

					MailAddress Addr = new MailAddress(s);
					Addresses.Add(Addr.Address);
				}

				foreach (string Part in NotificationUrls.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries))
				{
					string s = Part.Trim();
					if (string.IsNullOrEmpty(s))
						continue;

					Uri Uri = new Uri(s);
					WebHooks.Add(Uri.ToString());
				}

				this.addresses = Addresses.ToArray();
				this.urls = WebHooks.ToArray();

				await Database.Update(this);

				if (this.addresses.Length > 0 || this.urls.Length > 0)
				{
					await Gateway.SendNotification("Test\r\n===========\r\n\r\nThis message was generated to test the notification feature of **" +
						MarkdownDocument.Encode(Gateway.ApplicationName) + "**.");
				}

				if (!string.IsNullOrEmpty(TabID))
					await Response.Write(1);
			}
			catch (Exception ex)
			{
				if (!string.IsNullOrEmpty(TabID))
					await Response.Write(0);
				else
				{
					await Response.SendResponse(new BadRequestException(ex.Message));
					return;
				}
			}

			await Response.SendResponse();
		}

		/// <summary>
		/// Simplified configuration by configuring simple default values.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public override Task<bool> SimplifiedConfiguration()
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// JIDs of operators of gateway.
		/// </summary>
		public const string GATEWAY_NOTIFICATION_JIDS = nameof(GATEWAY_NOTIFICATION_JIDS);

		/// <summary>
		/// Webhook URLs of operators of gateway.
		/// </summary>
		public const string GATEWAY_NOTIFICATION_URLS = nameof(GATEWAY_NOTIFICATION_URLS);

		/// <summary>
		/// Environment configuration by configuring values available in environment variables.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public override Task<bool> EnvironmentConfiguration()
		{
			bool ValuesSet = false;
			CaseInsensitiveString Value = Environment.GetEnvironmentVariable(GATEWAY_NOTIFICATION_JIDS);

			if (!CaseInsensitiveString.IsNullOrEmpty(Value))
			{
				CaseInsensitiveString[] Jids = Value.Split(',');
				foreach (CaseInsensitiveString Jid in Jids)
				{
					if (!XmppClient.BareJidRegEx.IsMatch(Jid))
					{
						this.LogEnvironmentError("Invalid JID.", GATEWAY_NOTIFICATION_JIDS, Jid);
						return Task.FromResult(false);
					}
				}

				this.addresses = Jids;
				ValuesSet = true;
			}

			Value = Environment.GetEnvironmentVariable(GATEWAY_NOTIFICATION_URLS);

			if (!CaseInsensitiveString.IsNullOrEmpty(Value))
			{
				CaseInsensitiveString[] Urls = Value.Split(',');
				foreach (CaseInsensitiveString Url in Urls)
				{
					if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
					{
						this.LogEnvironmentError("Invalid URL.", GATEWAY_NOTIFICATION_URLS, Url);
						return Task.FromResult(false);
					}
				}

				this.urls = Urls;
				ValuesSet = true;
			}

			return Task.FromResult(ValuesSet);
		}

	}
}
