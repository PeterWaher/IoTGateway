namespace Waher.IoTGateway.Svc.ServiceManagement.Enumerations
{
	/// <summary>
	/// Service controller action type
	/// </summary>
	public enum ScActionType
	{
		/// <summary>
		/// None
		/// </summary>
		ScActionNone = 0,

		/// <summary>
		/// Restart
		/// </summary>
		ScActionRestart = 1,

		/// <summary>
		/// Reboot
		/// </summary>
		ScActionReboot = 2,

		/// <summary>
		/// Run command
		/// </summary>
		ScActionRunCommand = 3,
	}
}
