namespace Waher.Events.Syslog
{
	/// <summary>
	/// How events are separated in the Syslog event stream.
	/// </summary>
	public enum SyslogEventSeparation
	{
		/// <summary>
		/// Using an Octet count, as defined in §3.4.1 in RFC 6587.
		/// </summary>
		OctetCounting,

		/// <summary>
		/// Using a Non-Transparent-Framing using Carriage Return and Line Feed (CRLF), 
		/// as defined in §3.4.2 in RFRC 6587.
		/// </summary>
		CrLf,

		/// <summary>
		/// Using a Non-Transparent-Framing using Line Feed (LF), as defined in 
		/// §3.4.2 in RFRC 6587.
		/// </summary>
		Lf,

		/// <summary>
		/// Using a Non-Transparent-Framing using a NULL character (0), as defined in 
		/// §3.4.2 in RFRC 6587.
		/// </summary>
		Null
	}
}
