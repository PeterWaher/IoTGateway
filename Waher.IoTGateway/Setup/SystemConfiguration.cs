using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Abstract base class for system configurations.
	/// </summary>
	[CollectionName("SystemConfigurations")]
	[ArchivingTime]
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
		/// Gets a title for the system configuration.
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Title string</returns>
		public abstract Task<string> Title(Language Language);

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
		/// <param name="WebServer">Current Web Server object.</param>
		public virtual Task InitSetup(HttpServer WebServer)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Unregisters the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public virtual Task UnregisterSetup(HttpServer WebServer)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Waits for the user to provide configuration.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		/// <returns>If all system configuration objects must be reloaded from the database.</returns>
		public virtual Task<bool> SetupConfiguration(HttpServer WebServer)
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
			if (!(this.configResource is null))
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
		protected virtual async Task ConfigComplete(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);
			
			await this.MakeCompleted();
			
			Response.StatusCode = 200;
		}

		/// <summary>
		/// Minimum required privilege for a user to be allowed to change the configuration defined by the class.
		/// </summary>
		protected abstract string ConfigPrivilege
		{
			get;
		}

		/// <summary>
		/// Sets the configuration task as completed.
		/// </summary>
		public virtual Task MakeCompleted()
		{
			return this.MakeCompleted(false);
		}

		/// <summary>
		/// Sets the configuration task as completed.
		/// </summary>
		/// <param name="ReloadConfiguration">If system configuration objects must be reloaded. (Default=false)</param>
		protected async Task MakeCompleted(bool ReloadConfiguration)
		{
			this.complete = true;
			this.completed = DateTime.Now;
			this.updated = DateTime.Now;

			if (!ReloadConfiguration)
			{
				if (this.Priority <= 0)
					await Gateway.InternalDatabase.Update(this);
				else
					await Database.Update(this);
			}

			this.completionSource?.SetResult(ReloadConfiguration);
		}

		/// <summary>
		/// Simplified configuration by configuring simple default values.
		/// </summary>
		/// <returns>If the configuration was changed.</returns>
		public virtual Task<bool> SimplifiedConfiguration()
		{
			return Task.FromResult<bool>(false);
		}
	}
}
