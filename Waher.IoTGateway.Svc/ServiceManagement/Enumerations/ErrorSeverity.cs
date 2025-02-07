namespace Waher.IoTGateway.Svc.ServiceManagement.Enumerations
{
	/// <summary>
	/// Error severity
	/// </summary>
	public enum ErrorSeverity : uint
	{
		/// <summary>
		/// Ignore
		/// </summary>
		Ignore = 0,

		/// <summary>
		/// Nornal
		/// </summary>
		Normal = 1,

		/// <summary>
		/// Severe
		/// </summary>
		Severe = 2,

		/// <summary>
		/// Critical
		/// </summary>
		Crititcal = 3
	}
}
