namespace Waher.IoTGateway.Svc.ServiceManagement.Enumerations
{
	/// <summary>
	/// Service control commands
	/// </summary>
	public enum ServiceControlCommand : uint
	{
		/// <summary>
		/// Stop service
		/// </summary>
		Stop = 0x00000001,

		/// <summary>
		/// Pause service
		/// </summary>
		Pause = 0x00000002,

		/// <summary>
		/// Continue service
		/// </summary>
		Continue = 0x00000003,

		/// <summary>
		/// Interrogate service
		/// </summary>
		Interrogate = 0x00000004,

		/// <summary>
		/// Shutdown service
		/// </summary>
		Shutdown = 0x00000005,

		/// <summary>
		/// Parameter change
		/// </summary>
		Paramchange = 0x00000006,

		/// <summary>
		/// net bind added
		/// </summary>
		NetBindAdd = 0x00000007,

		/// <summary>
		/// net bind removed
		/// </summary>
		NetBindRemoved = 0x00000008,

		/// <summary>
		/// Net bind enabled
		/// </summary>
		NetBindEnable = 0x00000009,

		/// <summary>
		/// net bind disabled
		/// </summary>
		NetBindDisable = 0x0000000A,

		/// <summary>
		/// Device event
		/// </summary>
		DeviceEvent = 0x0000000B,

		/// <summary>
		/// Hardware profile change
		/// </summary>
		HardwareProfileChange = 0x0000000C,

		/// <summary>
		/// Power event
		/// </summary>
		PowerEvent = 0x0000000D,

		/// <summary>
		/// Session change
		/// </summary>
		SessionChange = 0x0000000E,

		/// <summary>
		/// Pre-shutdown
		/// </summary>
		PreShutdown = 0x0000000F,

		/// <summary>
		/// Time changed
		/// </summary>
		TimeChange = 0x00000010,

		/// <summary>
		/// Trigger event
		/// </summary>
		TriggerEvent = 0x00000020,

		/// <summary>
		/// User mode reboot
		/// </summary>
		UserModeReboot = 0x00000040
	}
}
