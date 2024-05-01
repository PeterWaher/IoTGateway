using System;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Runtime.Language;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Provides a short introduction to the system configuration process.
	/// </summary>
	public class Introduction : SystemConfiguration
	{
		private static Introduction instance = null;

		private HttpResource simplified = null;

		/// <summary>
		/// Current instance of configuration.
		/// </summary>
		public static Introduction Instance => instance;

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public override string Resource => "/Settings/Introduction.md";

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public override int Priority => -100;

		/// <summary>
		/// Gets a title for the system configuration.
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Title string</returns>
		public override Task<string> Title(Language Language)
		{
			return Language.GetStringAsync(typeof(Gateway), 8, "Introduction");
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
			instance = Configuration as Introduction;
		}

		/// <summary>
		/// Waits for the user to provide configuration.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		/// <returns>If all system configuration objects must be reloaded from the database.</returns>
		public override Task<bool> SetupConfiguration(HttpServer WebServer)
		{
			this.simplified = WebServer.Register("/Settings/Simplified", null, this.Simplified, true, false, true);

			return base.SetupConfiguration(WebServer);
		}

		/// <summary>
		/// Cleans up after configuration has been performed.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task CleanupAfterConfiguration(HttpServer WebServer)
		{
			WebServer.Unregister(this.simplified);

			return base.CleanupAfterConfiguration(WebServer);
		}

		/// <summary>
		/// Minimum required privilege for a user to be allowed to change the configuration defined by the class.
		/// </summary>
		protected override string ConfigPrivilege => "Admin.Config.Introduction";

		private async Task Simplified(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			await Gateway.SimplifiedConfiguration();

			Response.StatusCode = 200;
			await this.MakeCompleted();

			await Response.SendResponse();
		}

		/// <summary>
		/// Environment variable name for simple setup configuration.
		/// </summary>
		public const string GATEWAY_SIMPLE_SETUP = nameof(GATEWAY_SIMPLE_SETUP);

		/// <summary>
		/// Environment configuration by configuring values available in environment variables.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public override async Task<bool> EnvironmentConfiguration()
		{
			if (!this.TryGetEnvironmentVariable(GATEWAY_SIMPLE_SETUP, false, out bool SimpleSetup))
				return false;

			if (SimpleSetup)
				await Gateway.SimplifiedConfiguration();
			
			return true;
		}

	}
}
