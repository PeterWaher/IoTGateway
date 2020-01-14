using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
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

		private CaseInsensitiveString[] addresses = new CaseInsensitiveString[0];

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
			get { return this.addresses; }
			set { this.addresses = value; }
		}

		/// <summary>
		/// A string containing all addresses, delimited by "; ".
		/// </summary>
		public string AddressesString
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				bool First = true;

				foreach (string s in this.addresses)
				{
					if (First)
						First = false;
					else
						sb.Append("; ");

					sb.Append(s);
				}

				return sb.ToString();
			}
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
			this.testAddresses = WebServer.Register("/Settings/TestNotificationAddresses", null, this.TestNotificationAddresses, false, false, true);

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

		private async void TestNotificationAddresses(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				Gateway.AssertUserAuthenticated(Request);

				if (!Request.HasData)
					throw new BadRequestException();

				object Obj = Request.DecodeData();
				if (!(Obj is string Address))
					throw new BadRequestException();

				string TabID = Request.Header["X-TabID"];
				if (string.IsNullOrEmpty(TabID))
					throw new BadRequestException();

				List<CaseInsensitiveString> Addresses = new List<CaseInsensitiveString>();

				Response.StatusCode = 200;

				try
				{
					foreach (string Part in Address.Split(';'))
					{
						string s = Part.Trim();
						if (string.IsNullOrEmpty(s))
							continue;

						MailAddress Addr = new MailAddress(s);
						Addresses.Add(Addr.Address);
					}

					this.addresses = Addresses.ToArray();

					await Database.Update(this);

					if (this.addresses.Length > 0)
						Gateway.SendNotification("Test\r\n===========\r\n\r\nThis message was generated to test the notification feature of **" + Gateway.ApplicationName + "**.");

					Response.Write(1);
				}
				catch (Exception)
				{
					Response.Write(0);
				}

				Response.SendResponse();
			}
			catch (Exception ex)
			{
				Response.SendResponse(ex);
			}
			finally
			{
				Response.Dispose();
			}
		}

		/// <summary>
		/// Simplified configuration by configuring simple default values.
		/// </summary>
		/// <returns>If the configuration was changed.</returns>
		public override Task<bool> SimplifiedConfiguration()
		{
			return Task.FromResult<bool>(true);
		}

	}
}
