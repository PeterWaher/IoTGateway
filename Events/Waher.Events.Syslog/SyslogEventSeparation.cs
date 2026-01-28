namespace Waher.Events.Syslog
{
	/// <summary>
	/// How events are separated in the Syslog event stream.
	/// </summary>
	public enum SyslogEventSeparation
	{
		/// <summary>
		/// Using an Octet count, as defined in RFC 6587.
		/// </summary>
		OctetCounting,

		/// <summary>
		/// Using a Carriage Return and Line Feed (CRLF), as defined in early versions
		/// of Syslog.
		/// </summary>
		CrLf
	}
}
