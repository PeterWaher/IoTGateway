using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;

namespace Waher.IoTGateway.Setup.Databases
{
	/// <summary>
	/// Plugin for MongoDB databases.
	/// </summary>
	public class MongoDBDatabase : IDatabasePlugin
	{
		/// <summary>
		/// Plugin for MongoDB databases.
		/// </summary>
		public MongoDBDatabase()
		{
		}

		/// <summary>
		/// Displayable name of database plug-in
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Displayable name.</returns>
		public string Name(Language Language)
		{
			return "MongoDB Database";
		}

		/// <summary>
		/// Creates a new settings object.
		/// </summary>
		/// <returns>Settings object.</returns>
		public DatabaseSettings CreateNewSettings()
		{
			return new MongoDBSettings();
		}

		/// <summary>
		/// Configures a database connection using the provided settings object.
		/// </summary>
		/// <param name="Settings">Settings object.</param>
		public Task ConfigureSettings(object Settings)
		{
			if (Settings is MongoDBSettings MongoDBSettings && !Database.Locked)
			{
				object MongoClientSettings = Types.CreateObject("MongoDB.Driver.MongoClientSettings");
				Types.SetProperty(MongoClientSettings, "ApplicationName", Gateway.ApplicationName);

				if (!string.IsNullOrEmpty(MongoDBSettings.Host))
				{
					object MongoServerAddress;

					if (MongoDBSettings.Port.HasValue)
						MongoServerAddress = Types.CreateObject("MongoDB.Driver.MongoServerAddress", MongoDBSettings.Host, MongoDBSettings.Port.Value);
					else
						MongoServerAddress = Types.CreateObject("MongoDB.Driver.MongoServerAddress", MongoDBSettings.Host);

					Types.SetProperty(MongoClientSettings, "Server", MongoServerAddress);
				}

				if (!string.IsNullOrEmpty(MongoDBSettings.UserName))
				{
					object MongoCredential = Types.CallStatic("MongoDB.Driver.MongoCredential", "CreateCredential", MongoDBSettings.Database,
						MongoDBSettings.UserName, MongoDBSettings.Password);

					Types.SetProperty(MongoClientSettings, "Credential", MongoCredential);
				}

				IDatabaseProvider MongoDBProvider = Types.CreateObject("Waher.Persistence.MongoDB.MongoDBProvider", MongoClientSettings,
					MongoDBSettings.Database, MongoDBSettings.DefaultCollection) as IDatabaseProvider;

				Database.Register(MongoDBProvider, true);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Resource name pointing to settings form.
		/// </summary>
		public string SettingsResource => "/Settings/Database/MongoDB.md";

		/// <summary>
		/// Tests database connection parameters.
		/// </summary>
		/// <param name="Form">Settings parameters provided in configuration form.</param>
		/// <param name="Save">If parameters should be saved, if OK.</param>
		/// <param name="Settings">Settings object for the plugin.</param>
		/// <exception cref="Exception">Exception, in case parameters are not correct.</exception>
		public async Task Test(Dictionary<string, object> Form, bool Save, object Settings)
		{
			if (!Form.TryGetValue("HostName", out object Obj) ||
				!(Obj is string HostName) ||
				!Form.TryGetValue("DatabaseName", out Obj) ||
				!(Obj is string DatabaseName) ||
				!Form.TryGetValue("DefaultCollection", out Obj) ||
				!(Obj is string DefaultCollection) ||
				!Form.TryGetValue("UserName", out Obj) ||
				!(Obj is string UserName) ||
				!Form.TryGetValue("Password", out Obj) ||
				!(Obj is string Password) ||
				!Form.TryGetValue("PortNumber", out Obj) ||
				!(Obj is string PortNumberString) ||
				!(Settings is MongoDBSettings MongoDBSettings))
			{
				throw new BadRequestException();
			}

			(string Message, string _, string _) = await this.Test(HostName, DatabaseName, DefaultCollection, UserName, Password,
				PortNumberString, Save, MongoDBSettings);

			if (!string.IsNullOrEmpty(Message))
				throw new BadRequestException(Message);
		}

		/// <summary>
		/// Environment variable name for MongoDB Host Name.
		/// </summary>
		public const string MONGO_DB_HOST = nameof(MONGO_DB_HOST);

		/// <summary>
		/// Environment variable name for MongoDB Database Name.
		/// </summary>
		public const string MONGO_DB_NAME = nameof(MONGO_DB_NAME);

		/// <summary>
		/// Environment variable name for MongoDB default collection.
		/// </summary>
		public const string MONGO_DB_DEFAULT = nameof(MONGO_DB_DEFAULT);

		/// <summary>
		/// Environment variable name for MongoDB User Name.
		/// </summary>
		public const string MONGO_DB_USER = nameof(MONGO_DB_USER);

		/// <summary>
		/// Environment variable name for MongoDB Password.
		/// </summary>
		public const string MONGO_DB_PASSWORD = nameof(MONGO_DB_PASSWORD);

		/// <summary>
		/// Environment variable name for MongoDB Port number.
		/// </summary>
		public const string MONGO_DB_PORT = nameof(MONGO_DB_PORT);

		private async Task<(string, string, string)> Test(string HostName, string DatabaseName, string DefaultCollection, string UserName, string Password,
			string PortNumberString, bool Save, MongoDBSettings Settings)
		{
			try
			{
				int? PortNumber;

				if (string.IsNullOrEmpty(PortNumberString))
					PortNumber = null;
				else if (!int.TryParse(PortNumberString, out int i) || i <= 0 || i > 65535)
					return ("Invalid Port Number.", MONGO_DB_PORT, PortNumberString);
				else
					PortNumber = i;

				object MongoClientSettings = Types.CreateObject("MongoDB.Driver.MongoClientSettings");
				Types.SetProperty(MongoClientSettings, "ApplicationName", Gateway.ApplicationName);
				Types.SetProperty(MongoClientSettings, "ConnectTimeout", TimeSpan.FromSeconds(5));

				if (!string.IsNullOrEmpty(HostName))
				{
					try
					{
						object MongoServerAddress;

						if (PortNumber.HasValue)
							MongoServerAddress = Types.CreateObject("MongoDB.Driver.MongoServerAddress", HostName, PortNumber.Value);
						else
							MongoServerAddress = Types.CreateObject("MongoDB.Driver.MongoServerAddress", HostName);

						Types.SetProperty(MongoClientSettings, "Server", MongoServerAddress);
					}
					catch (Exception ex)
					{
						return (ex.Message, MONGO_DB_HOST, HostName);
					}
				}

				if (!string.IsNullOrEmpty(Settings.UserName))
				{
					try
					{
						object MongoCredential = Types.CallStatic("MongoDB.Driver.MongoCredential", "CreateCredential",
							DatabaseName, UserName, Password);

						Types.SetProperty(MongoClientSettings, "Credential", MongoCredential);
					}
					catch (Exception ex)
					{
						return (ex.Message, MONGO_DB_USER, UserName);
					}
				}

				object Client = Types.CreateObject("MongoDB.Driver.MongoClient", MongoClientSettings);

				Task Task = Types.Call(Client, "StartSessionAsync") as Task;
				await Task;

				Types.Call(Client, "GetDatabase", DatabaseName);

				if (Save)
				{
					Settings.Host = HostName;
					Settings.Port = PortNumber;
					Settings.UserName = UserName;
					Settings.Password = Password;
					Settings.Database = DatabaseName;
					Settings.DefaultCollection = DefaultCollection;
				}
			}
			catch (Exception ex)
			{
				return (ex.Message, MONGO_DB_HOST, HostName);
			}

			return (null, null, null);
		}

		/// <summary>
		/// Tests database connection parameters available via environment variables.
		/// </summary>
		/// <param name="Configuration">Configuration object.</param>
		/// <param name="Settings">Plugin settings parameters.</param>
		/// <returns>If environment configuration is correct.</returns>
		public async Task<bool> TestEnvironmentVariables(DatabaseConfiguration Configuration, object Settings)
		{
			if (!(Settings is MongoDBSettings MongoDBSettings))
				return false;

			if (!Configuration.TryGetEnvironmentVariable(MONGO_DB_HOST, false, out string Host))
				return false;

			if (!Configuration.TryGetEnvironmentVariable(MONGO_DB_NAME, true, out string Name) ||
				!Configuration.TryGetEnvironmentVariable(MONGO_DB_USER, true, out string User) ||
				!Configuration.TryGetEnvironmentVariable(MONGO_DB_PASSWORD, true, out string Password) ||
				!Configuration.TryGetEnvironmentVariable(MONGO_DB_PORT, true, out string Port))
			{
				return false;
			}

			if (!Configuration.TryGetEnvironmentVariable(MONGO_DB_DEFAULT, false, out string Default))
				Default = "Default";

			(string Message, string Parameter, string Value) = await this.Test(Host, Name, Default, User, Password, Port, true, MongoDBSettings);

			if (!string.IsNullOrEmpty(Message))
			{
				Configuration.LogEnvironmentError(Message, Parameter, Value);
				return false;
			}

			return true;
		}

	}
}
