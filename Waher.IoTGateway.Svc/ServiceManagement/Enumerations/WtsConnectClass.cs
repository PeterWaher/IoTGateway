namespace Waher.IoTGateway.Svc.ServiceManagement.Enumerations
{
	/// <summary>
	/// WTS_CONNECTSTATE_CLASS
	/// Contains int values that indicate the connection state of a Terminal Services session.
	/// </summary>
	public enum WtsConnectClass
	{
		/// <summary>
		/// WTSActive
		/// </summary>
		WTSActive,

		/// <summary>
		/// WTSConnected
		/// </summary>
		WTSConnected,

		/// <summary>
		/// WTSConnectQuery
		/// </summary>
		WTSConnectQuery,

		/// <summary>
		/// WTSShadow
		/// </summary>
		WTSShadow,

		/// <summary>
		/// WTSDisconnected
		/// </summary>
		WTSDisconnected,

		/// <summary>
		/// WTSIdle
		/// </summary>
		WTSIdle,

		/// <summary>
		/// WTSListen
		/// </summary>
		WTSListen,

		/// <summary>
		/// WTSReset
		/// </summary>
		WTSReset,

		/// <summary>
		/// WTSDown
		/// </summary>
		WTSDown,

		/// <summary>
		/// WTSInit
		/// </summary>
		WTSInit
	}
}
