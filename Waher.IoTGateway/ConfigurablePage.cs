namespace Waher.IoTGateway
{
	/// <summary>
	/// Configuration page for a configurable module.
	/// </summary>
	public class ConfigurablePage : IConfigurablePage
	{
		/// <summary>
		/// Configuration page for a configurable module.
		/// </summary>
		/// <param name="Title">Title of configuration command.</param>
		/// <param name="ConfigurationPage">Local URL to configuration page.</param>
		/// <param name="Privileges">Privileges required to access the configuration page.</param>
		public ConfigurablePage(string Title, string ConfigurationPage, params string[] Privileges)
		{
			this.Title = Title;
			this.ConfigurationPage = ConfigurationPage;
			this.Privileges = Privileges;
		}

		/// <summary>
		/// Title of configuration command.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// Local URL to configuration page.
		/// </summary>
		public string ConfigurationPage { get; }

		/// <summary>
		/// Privileges required to access the configuration page.
		/// </summary>
		public string[] Privileges { get; }
	}
}
