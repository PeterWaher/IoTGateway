using System.Threading.Tasks;
using Waher.IoTGateway;
using Waher.Networking.HTTP;
using Waher.Runtime.Inventory;
using Waher.Things.Http;

namespace Waher.WebService.Queue
{
	/// <summary>
	/// Pluggable module registering a web service endpoint to access local queues.
	/// </summary>
	[Singleton]
	public class QueueServiceModule : IModule
	{
		internal const string RootPrivilege = "Admin.Queues.";

		private QueueWebService queueEndpoint;

		/// <summary>
		/// Pluggable module registering a web service endpoint to access local queues.
		/// </summary>
		public QueueServiceModule()
		{
		}

		/// <summary>
		/// Starts the module.
		/// </summary>
		public Task Start()
		{
			HttpAuthenticationScheme[] AuthenticationSchemes = HttpModule.GetAuthenticationSchemes();

			this.queueEndpoint = new QueueWebService("/Queues", AuthenticationSchemes);
			Gateway.HttpServer?.Register(this.queueEndpoint);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			if (!(Gateway.HttpServer is null))
			{
				Gateway.HttpServer.Unregister(this.queueEndpoint);
				this.queueEndpoint = null;
			}

			return Task.CompletedTask;
		}
	}
}
