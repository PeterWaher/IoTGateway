using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Runtime.Language;

namespace Waher.IoTGateway.Setup.Databases
{
	/// <summary>
	/// Plugin for the local database.
	/// </summary>
	public class InternalDatabase : IDatabasePlugin
	{
		/// <summary>
		/// Plugin for the local database.
		/// </summary>
		public InternalDatabase()
		{
		}

		/// <summary>
		/// Displayable name of database plug-in
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Displayable name.</returns>
		public string Name(Language Language)
		{
			return "Internal Database";
		}

		/// <summary>
		/// Creates a new settings object.
		/// </summary>
		/// <returns>Settings object.</returns>
		public IDatabaseSettings CreateNewSettings()
		{
			return new InternalSettings();
		}

		/// <summary>
		/// Configures a database connection using the provided settings object.
		/// </summary>
		/// <param name="Settings">Settings object.</param>
		public Task ConfigureSettings(object Settings)
		{
			Database.Register(Database.Provider, true);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Resource name pointing to settings form.
		/// </summary>
		public string SettingsResource => string.Empty;

		/// <summary>
		/// Tests database connection parameters.
		/// </summary>
		/// <param name="Form">Settings parameters provided in configuration form.</param>
		/// <param name="Save">If parameters should be saved, if OK.</param>
		/// <param name="Settings">Settings object for the plugin.</param>
		/// <exception cref="Exception">Exception, in case parameters are not correct.</exception>
		public Task Test(Dictionary<string, object> Form, bool Save, object Settings)
		{
			return Task.CompletedTask;
		}

	}
}
