using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
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
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// If the configuration is complete.
		/// </summary>
		[DefaultValue(false)]
		public bool Complete
		{
			get => this.complete;
			set => this.complete = value;
		}

		/// <summary>
		/// When the object was created.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Created
		{
			get => this.created;
			set => this.created = value;
		}

		/// <summary>
		/// When the object was updated.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Updated
		{
			get => this.updated;
			set => this.updated = value;
		}

		/// <summary>
		/// When the configuration was completed.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Completed
		{
			get => this.completed;
			set => this.completed = value;
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
			Gateway.AssertUserAuthenticated(Request, new string[] { this.ConfigPrivilege });

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
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public virtual Task<bool> SimplifiedConfiguration()
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Environment configuration by configuring values available in environment variables.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public abstract Task<bool> EnvironmentConfiguration();

		/// <summary>
		/// Logs an error to the event log, telling the operator an environment variable value
		/// contains an error.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Value">Value of environment variable.</param>
		public void LogEnvironmentError(string EnvironmentVariable, object Value)
		{
			this.LogEnvironmentError(null, EnvironmentVariable, Value);
		}

		/// <summary>
		/// Logs an error to the event log, telling the operator an environment variable value is missing.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Value">Value of environment variable.</param>
		public void LogEnvironmentVariableMissingError(string EnvironmentVariable, object Value)
		{
			this.LogEnvironmentError("Value missing.", EnvironmentVariable, Value);
		}

		/// <summary>
		/// Logs an error to the event log, telling the operator an environment variable value
		/// is not a valid Boolean value.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Value">Value of environment variable.</param>
		public void LogEnvironmentVariableInvalidBooleanError(string EnvironmentVariable, object Value)
		{
			this.LogEnvironmentError("Invalid Boolean Value.", EnvironmentVariable, Value);
		}

		/// <summary>
		/// Logs an error to the event log, telling the operator an environment variable value
		/// is not a valid integer value.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Value">Value of environment variable.</param>
		public void LogEnvironmentVariableInvalidIntegerError(string EnvironmentVariable, object Value)
		{
			this.LogEnvironmentError("Invalid integer Value.", EnvironmentVariable, Value);
		}

		/// <summary>
		/// Logs an error to the event log, telling the operator an environment variable value
		/// is not a valid time value.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Value">Value of environment variable.</param>
		public void LogEnvironmentVariableInvalidTimeError(string EnvironmentVariable, object Value)
		{
			this.LogEnvironmentError("Invalid time Value.", EnvironmentVariable, Value);
		}

		/// <summary>
		/// Logs an error to the event log, telling the operator an environment variable value
		/// is not a valid date value.
		/// </summary>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Value">Value of environment variable.</param>
		public void LogEnvironmentVariableInvalidDateError(string EnvironmentVariable, object Value)
		{
			this.LogEnvironmentError("Invalid date Value.", EnvironmentVariable, Value);
		}

		/// <summary>
		/// Logs an error to the event log, telling the operator an environment variable value
		/// is not within a valid range.
		/// </summary>
		/// <param name="Min">Minimum value.</param>
		/// <param name="Max">Maximum value.</param>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Value">Value of environment variable.</param>
		public void LogEnvironmentVariableInvalidRangeError(int Min, int Max, string EnvironmentVariable, object Value)
		{
			if (Max == int.MaxValue)
			{
				this.LogEnvironmentError("Value not in valid range. Must be at least " + Min.ToString() + ".",
					EnvironmentVariable, Value);
			}
			else if (Min == int.MinValue)
			{
				this.LogEnvironmentError("Value not in valid range. Must be at most " + Max.ToString() + ".",
					EnvironmentVariable, Value);
			}
			else
			{
				this.LogEnvironmentError("Value not in valid range. Must be between " + Min.ToString() + " and " + Max.ToString() + ".",
					EnvironmentVariable, Value);
			}
		}

		/// <summary>
		/// Logs an error to the event log, telling the operator an environment variable value
		/// contains an error.
		/// </summary>
		/// <param name="Message">Message to log.</param>
		/// <param name="EnvironmentVariable">Name of environment variable.</param>
		/// <param name="Value">Value of environment variable.</param>
		public void LogEnvironmentError(string Message, string EnvironmentVariable, object Value)
		{
			if (string.IsNullOrEmpty(Message))
				Message = "Environment Variable contains an invalid value.";
			else
				Message = "Environment Variable contains an invalid value: " + Message;

			Log.Error(Message, EnvironmentVariable, string.Empty, "ConfigError",
				new KeyValuePair<string, object>("EnvironmentVariable", EnvironmentVariable),
				new KeyValuePair<string, object>("Value", Value));
		}

		/// <summary>
		/// Tries to get a string-valued environment variable.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Required">If variable is required.</param>
		/// <param name="Value">Value of environment variable.</param>
		/// <returns>If environment variable was found as was non-empty.</returns>
		public bool TryGetEnvironmentVariable(string VariableName, bool Required, out string Value)
		{
			Value = Environment.GetEnvironmentVariable(VariableName) ?? string.Empty;
			if (!string.IsNullOrEmpty(Value))
				return true;

			if (Required)
				this.LogEnvironmentVariableMissingError(VariableName, Value);

			return false;
		}

		/// <summary>
		/// Tries to get a Boolean-valued environment variable.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Required">If variable is required.</param>
		/// <param name="Value">Value of environment variable.</param>
		/// <returns>If environment variable was found and valid.</returns>
		public bool TryGetEnvironmentVariable(string VariableName, bool Required, out bool Value)
		{
			if (!this.TryGetEnvironmentVariable(VariableName, Required, out string s))
			{
				Value = default;
				return false;
			}

			if (!CommonTypes.TryParse(s, out Value))
			{
				this.LogEnvironmentVariableInvalidBooleanError(VariableName, s);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Tries to get a Boolean-valued environment variable.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Default">Default value, in case parameter is not defined.</param>
		/// <param name="Value">Value of environment variable.</param>
		/// <returns>If environment variable was found and valid.</returns>
		public bool TryGetEnvironmentVariable(string VariableName, out bool Value, bool Default)
		{
			if (!this.TryGetEnvironmentVariable(VariableName, false, out string s))
			{
				Value = Default;
				return true;
			}

			if (!CommonTypes.TryParse(s, out Value))
			{
				this.LogEnvironmentVariableInvalidBooleanError(VariableName, s);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Tries to get a integer-valued environment variable.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Required">If variable is required.</param>
		/// <param name="Value">Value of environment variable.</param>
		/// <returns>If environment variable was found and valid.</returns>
		public bool TryGetEnvironmentVariable(string VariableName, bool Required, out int Value)
		{
			if (!this.TryGetEnvironmentVariable(VariableName, Required, out string s))
			{
				Value = default;
				return false;
			}

			if (!int.TryParse(s, out Value))
			{
				this.LogEnvironmentVariableInvalidIntegerError(VariableName, s);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Tries to get a integer-valued environment variable within a range.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Min">Minimum value.</param>
		/// <param name="Max">Maximum value.</param>
		/// <param name="Required">If variable is required.</param>
		/// <param name="Value">Value of environment variable.</param>
		/// <returns>If environment variable was found and valid.</returns>
		public bool TryGetEnvironmentVariable(string VariableName, int Min, int Max, bool Required, ref int Value)
		{
			if (!this.TryGetEnvironmentVariable(VariableName, Required, out int i))
				return false;

			if (i < Min || i > Max)
			{
				this.LogEnvironmentVariableInvalidRangeError(Min, Max, VariableName, i);
				return false;
			}

			Value = i;

			return true;
		}

		/// <summary>
		/// Tries to get a time-valued environment variable.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Required">If variable is required.</param>
		/// <param name="Value">Value of environment variable.</param>
		/// <returns>If environment variable was found and valid.</returns>
		public bool TryGetEnvironmentVariable(string VariableName, bool Required, out TimeSpan? Value)
		{
			if (!this.TryGetEnvironmentVariable(VariableName, Required, out string s))
			{
				Value = default;
				return false;
			}

			if (!TimeSpan.TryParse(s, out TimeSpan TS) || TS < TimeSpan.Zero || TS.TotalHours >= 24)
			{
				this.LogEnvironmentVariableInvalidTimeError(VariableName, s);
				Value = default;
				return false;
			}

			Value = TS;
			return true;
		}

		/// <summary>
		/// Tries to get a date-valued environment variable.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Required">If variable is required.</param>
		/// <param name="Value">Value of environment variable.</param>
		/// <returns>If environment variable was found and valid.</returns>
		public bool TryGetEnvironmentVariable(string VariableName, bool Required, out DateTime? Value)
		{
			if (!this.TryGetEnvironmentVariable(VariableName, Required, out string s))
			{
				Value = default;
				return false;
			}

			if (!XML.TryParse(s, out DateTime TP) || TP.TimeOfDay != TimeSpan.Zero)
			{
				this.LogEnvironmentVariableInvalidDateError(VariableName, s);
				Value = default;
				return false;
			}

			Value = TP;
			return true;
		}

	}
}
