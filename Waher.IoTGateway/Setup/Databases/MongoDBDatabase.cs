using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.MongoDB;
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
		public IDatabaseSettings CreateNewSettings()
		{
			return new MongoDBSettings();
		}

		/// <summary>
		/// Configures a database connection using the provided settings object.
		/// </summary>
		/// <param name="Settings">Settings object.</param>
		public Task ConfigureSettings(object Settings)
		{
			if (Settings is MongoDBSettings MongoDBSettings)
			{
				MongoClientSettings MongoClientSettings = new MongoClientSettings()
				{
					ApplicationName = Gateway.ApplicationName
				};

				if (!string.IsNullOrEmpty(MongoDBSettings.Host))
				{
					if (MongoDBSettings.Port.HasValue)
						MongoClientSettings.Server = new MongoServerAddress(MongoDBSettings.Host, MongoDBSettings.Port.Value);
					else
						MongoClientSettings.Server = new MongoServerAddress(MongoDBSettings.Host);
				}

				if (!string.IsNullOrEmpty(MongoDBSettings.UserName))
				{
					MongoClientSettings.Credential = MongoCredential.CreateCredential(MongoDBSettings.Database,
						MongoDBSettings.UserName, MongoDBSettings.Password);
				}

				Database.Register(new MongoDBProvider(MongoClientSettings, MongoDBSettings.Database, MongoDBSettings.DefaultCollection), true);
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

			MongoClientSettings MongoClientSettings = new MongoClientSettings()
			{
				ApplicationName = Gateway.ApplicationName,
				ConnectTimeout = TimeSpan.FromSeconds(5)
			};

			if (!string.IsNullOrEmpty(HostName))
			{
				if (PortNumber.HasValue)
					MongoClientSettings.Server = new MongoServerAddress(HostName, PortNumber.Value);
				else
					MongoClientSettings.Server = new MongoServerAddress(HostName);
			}

			if (!string.IsNullOrEmpty(UserName))
				MongoClientSettings.Credential = MongoCredential.CreateCredential(DatabaseName, UserName, Password);

			MongoClient Client = new MongoClient(MongoClientSettings);

			await Client.StartSessionAsync();
			IMongoDatabase Database = Client.GetDatabase(DatabaseName);

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
