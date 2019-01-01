using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.Enumerations
{
	/// <summary>
	/// TYPE fields are used in resource records.
	/// </summary>
	public enum TYPE
	{
		/// <summary>
		/// a host address
		/// </summary>
		A = 1,

		/// <summary>
		/// An authoritative name server
		/// </summary>
		NS = 2,

		/// <summary>
		/// a mail destination (Obsolete - use MX)
		/// </summary>
		MD = 3,

		/// <summary>
		/// a mail forwarder (Obsolete - use MX)
		/// </summary>
		MF = 4,

		/// <summary>
		/// the canonical name for an alias
		/// </summary>
		CNAME = 5,

		/// <summary>
		/// marks the start of a zone of authority
		/// </summary>
		SOA = 6,

		/// <summary>
		/// a mailbox domain name (EXPERIMENTAL)
		/// </summary>
		MB = 7,

		/// <summary>
		/// a mail group member (EXPERIMENTAL)
		/// </summary>
		MG = 8,

		/// <summary>
		/// a mail rename domain name (EXPERIMENTAL)
		/// </summary>
		MR = 9,

		/// <summary>
		/// a null RR (EXPERIMENTAL)
		/// </summary>
		NULL = 10,

		/// <summary>
		/// a well known service description
		/// </summary>
		WKS = 11,

		/// <summary>
		/// a domain name pointer
		/// </summary>
		PTR = 12,

		/// <summary>
		/// host information
		/// </summary>
		HINFO = 13,

		/// <summary>
		/// mailbox or mail list information
		/// </summary>
		MINFO = 14,

		/// <summary>
		/// mail exchange
		/// </summary>
		MX = 15,

		/// <summary>
		///  text strings
		/// </summary>
		TXT = 16
	}
}
