using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.Enumerations
{
	/// <summary>
	/// QCLASS fields appear in the question section of a query.
	/// </summary>
	public enum QCLASS
	{
		/// <summary>
		/// the Internet
		/// </summary>
		IN = 1,

		/// <summary>
		/// the CSNET class (Obsolete - used only for examples in some obsolete RFCs)
		/// </summary>
		CS = 2,

		/// <summary>
		/// the CHAOS class
		/// </summary>
		CH = 3,

		/// <summary>
		/// Hesiod[Dyer 87]
		/// </summary>
		HS = 4,

		/// <summary>
		/// (*) any class
		/// </summary>
		Wildcard = 255
	}
}
