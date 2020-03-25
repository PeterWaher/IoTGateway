using System;
using System.Collections.Generic;
using System.Reflection;
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

			int? PortNumber;

			if (string.IsNullOrEmpty(PortNumberString))
				PortNumber = null;
			else if (!int.TryParse(PortNumberString, out int i) || i <= 0 || i > 65535)
				throw new BadRequestException("Invalid Port Number.");
			else
				PortNumber = i;


			object MongoClientSettings = Types.CreateObject("MongoDB.Driver.MongoClientSettings");
			Types.SetProperty(MongoClientSettings, "ApplicationName", Gateway.ApplicationName);
			Types.SetProperty(MongoClientSettings, "ConnectTimeout", TimeSpan.FromSeconds(5));

			if (!string.IsNullOrEmpty(HostName))
			{
				object MongoServerAddress;

				if (PortNumber.HasValue)
					MongoServerAddress = Types.CreateObject("MongoDB.Driver.MongoServerAddress", HostName, PortNumber.Value);
				else
					MongoServerAddress = Types.CreateObject("MongoDB.Driver.MongoServerAddress", HostName);

				Types.SetProperty(MongoClientSettings, "Server", MongoServerAddress);
			}

			if (!string.IsNullOrEmpty(MongoDBSettings.UserName))
			{
				object MongoCredential = Types.CallStatic("MongoDB.Driver.MongoCredential", "CreateCredential", 
					DatabaseName, UserName, Password);

				Types.SetProperty(MongoClientSettings, "Credential", MongoCredential);
			}

			object Client = Types.CreateObject("MongoDB.Driver.MongoClient", MongoClientSettings);

			Task Task = Types.Call(Client, "StartSessionAsync") as Task;
			await Task;

			Types.Call(Client, "GetDatabase", DatabaseName);

			if (Save)
			{
				MongoDBSettings.Host = HostName;
				MongoDBSettings.Port = PortNumber;
				MongoDBSettings.UserName = UserName;
				MongoDBSettings.Password = Password;
				MongoDBSettings.Database = DatabaseName;
				MongoDBSettings.DefaultCollection = DefaultCollection;
			}
		}

	}
}
