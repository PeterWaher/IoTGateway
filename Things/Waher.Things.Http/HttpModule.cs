using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Runtime.Inventory;

namespace Waher.Things.Http
{
	/// <summary>
	/// HTTP module
	/// </summary>
	[Singleton]
	public class HttpModule : IModule
	{
		private HttpServer webServer;
		private SensorDataReceptorResource api;

		/// <summary>
		/// Starts the module.
		/// </summary>
		public Task Start()
		{
			try
			{
				if (Types.TryGetModuleParameter("HTTP", out object Obj) &&
					Obj is HttpServer WebServer)
				{
					this.webServer = WebServer;
					this.api = new SensorDataReceptorResource("/ReportSensorData");
					this.webServer.Register(this.api);
				}
				else
					Log.Error("Local Web Server not found.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			if (!(this.webServer is null) && !(this.api is null))
			{
				this.webServer.Unregister(this.api);
				this.webServer = null;
				this.api = null;
			}

			return Task.CompletedTask;
		}
	}
}
