using System;
using System.Threading;
using Waher.Networking.HTTP;
using Waher.Runtime.Inventory;

namespace Waher.WebService.Script
{
	/// <summary>
	/// Pluggable module registering the script service to the web server.
	/// </summary>
    public class ScriptServiceModule : IModule
	{
		private HttpServer webServer;
		private ScriptService instance;

		/// <summary>
		/// Pluggable module registering the script service to the web server.
		/// </summary>
		public ScriptServiceModule()
		{
		}

		/// <summary>
		/// Starts the module.
		/// </summary>
		/// <returns>If an asynchronous start operation has been started, a wait handle is returned. This
		/// wait handle can be used to wait for the asynchronous process to finish. If no such asynchronous
		/// operation has been started, null can be returned.</returns>
		public WaitHandle Start()
		{
			if (Types.TryGetModuleParameter("HTTP", out object Obj) && (this.webServer = Obj as HttpServer) != null)
				this.webServer.Register(this.instance = new ScriptService("/Evaluate"));  // TODO: Add authentication mechanisms.

			return null;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public void Stop()
		{
			if (this.webServer != null)
			{
				this.webServer.Unregister(this.instance);

				this.webServer = null;
				this.instance = null;
			}
		}
	}
}
