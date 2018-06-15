using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Abstract base class for system configurations.
	/// </summary>
	[CollectionName("SystemConfigurations")]
	public abstract class SystemConfiguration : ISystemConfiguration
	{
		private TaskCompletionSource<bool> completionSource = null;
		private HttpResource configResource = null;

		private Guid objectId = Guid.Empty;
		private DateTime created = DateTime.MinValue;
		private DateTime updated = DateTime.MinValue;
		private DateTime completed = DateTime.MinValue;
		private bool complete = false;

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public Guid ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// If the configuration is complete.
		/// </summary>
		[DefaultValue(false)]
		public bool Complete
		{
			get { return this.complete; }
			set { this.complete = value; }
		}

		/// <summary>
		/// When the object was created.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Created
		{
			get { return this.created; }
			set { this.created = value; }
		}

		/// <summary>
		/// When the object was updated.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Updated
		{
			get { return this.updated; }
			set { this.updated = value; }
		}

		/// <summary>
		/// When the configuration was completed.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Completed
		{
			get { return this.completed; }
			set { this.completed = value; }
		}

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public abstract string Resource
		{
			get;
		}

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public abstract int Priority
		{
			get;
		}

		/// <summary>
		/// Is called during startup to configure the system.
		/// </summary>
		public abstract Task ConfigureSystem();

		/// <summary>
		/// Sets the static instance of the configuration.
		/// </summary>
		/// <param name="Configuration">Configuration object</param>
		public abstract void SetStaticInstance(ISystemConfiguration Configuration);

		/// <summary>
		/// Initializes the setup object.
		/// </summary>
		public virtual Task InitSetup()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Waits for the user to provide configuration.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public virtual Task SetupConfiguration(HttpServer WebServer)
		{
			this.completionSource = new TaskCompletionSource<bool>();

			this.configResource = WebServer.Register("/Settings/ConfigComplete", null, this.ConfigComplete, true, false, true);

			return this.completionSource.Task;
		}

		/// <summary>
		/// Cleans up after configuration has been performed.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public virtual Task CleanupAfterConfiguration(HttpServer WebServer)
		{
			if (this.configResource != null)
			{
				WebServer.Unregister(this.configResource);
				this.configResource = null;
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Method is called when the user completes the current configuration task.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		protected virtual async void ConfigComplete(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			Response.StatusCode = 200;
			await this.MakeCompleted();
		}

		/// <summary>
		/// Sets the configuration task as completed.
		/// </summary>
		public virtual async Task MakeCompleted()
		{
			this.complete = true;
			this.completed = DateTime.Now;

			await Database.Update(this);

			this.completionSource?.SetResult(true);
		}
	}
}
