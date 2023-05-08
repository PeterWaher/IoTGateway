namespace Waher.IoTGateway
{
	/// <summary>
	/// Configuration page for a configurable module.
	/// </summary>
	public interface IConfigurablePage
	{
		/// <summary>
		/// Title of configuration command.
		/// </summary>
		string Title { get; }

		/// <summary>
		/// Local URL to configuration page.
		/// </summary>
		string ConfigurationPage { get; }

		/// <summary>
		/// Privileges required to access the configuration page.
		/// </summary>
		string[] Privileges { get; }
	}
}
