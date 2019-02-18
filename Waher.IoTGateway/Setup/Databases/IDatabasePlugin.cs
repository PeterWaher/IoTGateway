using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Language;

namespace Waher.IoTGateway.Setup.Databases
{
	/// <summary>
	/// Interface for Database plug-ins
	/// </summary>
	public interface IDatabasePlugin
	{
		/// <summary>
		/// Displayable name of database plug-in
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Displayable name.</returns>
		string Name(Language Language);

		/// <summary>
		/// Resource name pointing to settings form.
		/// </summary>
		string SettingsResource
		{
			get;
		}

		/// <summary>
		/// Creates a new settings object.
		/// </summary>
		/// <returns>Settings object.</returns>
		DatabaseSettings CreateNewSettings();

		/// <summary>
		/// Configures a database connection using the provided settings object.
		/// </summary>
		/// <param name="Settings">Settings object.</param>
		Task ConfigureSettings(object Settings);

		/// <summary>
		/// Tests database connection parameters.
		/// </summary>
		/// <param name="Form">Settings parameters provided in configuration form.</param>
		/// <param name="Save">If parameters should be saved, if OK.</param>
		/// <param name="Settings">Settings object for the plugin.</param>
		/// <exception cref="Exception">Exception, in case parameters are not correct.</exception>
		Task Test(Dictionary<string, object> Form, bool Save, object Settings);
	}
}
