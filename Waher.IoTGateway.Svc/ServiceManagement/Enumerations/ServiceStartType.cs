namespace Waher.IoTGateway.Svc.ServiceManagement.Enumerations
{
	/// <summary>
	/// How the windows service should be started.
	/// </summary>
	public enum ServiceStartType : uint
	{
		/// <summary>
		/// Start on boot
		/// </summary>
		StartOnBoot = 0,

		/// <summary>
		/// Start on system start
		/// </summary>
		StartOnSystemStart = 1,

		/// <summary>
		/// Auto-start
		/// </summary>
		AutoStart = 2,

		/// <summary>
		/// Start on demand
		/// </summary>
		StartOnDemand = 3,

		/// <summary>
		/// Disabled
		/// </summary>
		Disabled = 4
	}
}
