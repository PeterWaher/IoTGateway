using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.Enumerations
{
	/// <summary>
	/// DNS Response Code
	/// </summary>
	public enum RCode
	{
		/// <summary>
		/// No Error [RFC1035]
		/// </summary>
		NoError = 0,

		/// <summary>
		/// Format Error [RFC1035]
		/// </summary>
		FormErr = 1,

		/// <summary>
		/// Server Failure [RFC1035]
		/// </summary>
		ServFail = 2,

		/// <summary>
		/// Non-Existent Domain [RFC1035]
		/// </summary>
		NXDomain = 3,

		/// <summary>
		/// Not Implemented [RFC1035]
		/// </summary>
		NotImp = 4,

		/// <summary>
		/// Query Refused [RFC1035]
		/// </summary>
		Refused = 5,

		/// <summary>
		/// Name Exists when it should not [RFC2136] [RFC6672]
		/// </summary>
		YXDomain = 6,

		/// <summary>
		/// RR Set Exists when it should not [RFC2136]
		/// </summary>
		YXRRSet = 7,

		/// <summary>
		/// RR Set that should exist does not [RFC2136]
		/// </summary>
		NXRRSet = 8,

		/// <summary>
		/// Server Not Authoritative for zone [RFC2136]
		/// Not Authorized [RFC2845]
		/// </summary>
		NotAuth = 9,

		/// <summary>
		/// Name not contained in zone [RFC2136]
		/// </summary>
		NotZone = 10,

		/// <summary>
		/// DSO-TYPE Not Implemented [RFC-ietf-dnsop-session-signal-20]
		/// </summary>
		DSOTYPENI = 11,

		/// <summary>
		/// Bad OPT Version [RFC6891]
		/// TSIG Signature Failure [RFC2845]
		/// </summary>
		BADVERS = 16,

		/// <summary>
		/// Key not recognized [RFC2845]
		/// </summary>
		BADKEY = 17,

		/// <summary>
		/// Signature out of time window [RFC2845]
		/// </summary>
		BADTIME = 18,

		/// <summary>
		/// Bad TKEY Mode [RFC2930]
		/// </summary>
		BADMODE = 19,

		/// <summary>
		/// Duplicate key name [RFC2930]
		/// </summary>
		BADNAME = 20,

		/// <summary>
		/// Algorithm not supported [RFC2930]
		/// </summary>
		BADALG = 21,

		/// <summary>
		/// Bad Truncation [RFC4635]
		/// </summary>
		BADTRUNC = 22,

		/// <summary>
		/// Bad/missing Server Cookie [RFC7873]
		/// </summary>
		BADCOOKIE = 23
	}
}
