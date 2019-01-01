using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.Enumerations
{
	/// <summary>
	/// DNS Operation Codes
	/// </summary>
	public enum OpCode
	{
		/// <summary>
		/// Query (RFC 1035)
		/// </summary>
		Query = 0,

		/// <summary>
		/// Inverse Query, Obsolete (RFC 3425)
		/// </summary>
		InverseQuery = 1,

		/// <summary>
		/// Status (RFC 1035)
		/// </summary>
		Status = 2,

		/// <summary>
		/// Notify (RFC 1996)
		/// </summary>
		Notify = 4,

		/// <summary>
		/// Update (RFC 2136)
		/// </summary>
		Update = 5,

		/// <summary>
		/// DNS Stateful Operations
		/// </summary>
		DnsStateful = 6
	}
}
