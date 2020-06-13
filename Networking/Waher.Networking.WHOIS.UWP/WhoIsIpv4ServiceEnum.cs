using System;

namespace Waher.Networking.WHOIS
{
	/// <summary>
	/// Enumeration of WHOIS Services available for IPv4 addresses.
	/// </summary>
	public enum WhoIsIpv4ServiceEnum
	{
		/// <summary>
		/// APNIC
		/// RDAP: https://rdap.apnic.net/
		/// </summary>
		whois_apnic_net = 0,

		/// <summary>
		/// RIPE NCC
		/// RDAP: https://rdap.db.ripe.net/
		/// </summary>
		whois_ripe_net = 1,

		/// <summary>
		/// Administered by ARIN
		/// RDAP: https://rdap.arin.net/registry
		/// </summary>
		whois_arin_net = 2,

		/// <summary>
		/// AFRINIC
		/// RDAP: https://rdap.afrinic.net/rdap/
		/// </summary>
		whois_afrinic_net = 3,

		/// <summary>
		/// LACNIC
		/// RDAP: https://rdap.lacnic.net/rdap/
		/// </summary>
		whois_lacnic_net = 4,

		/// <summary>
		/// WHOIS Service undefined.
		/// </summary>
		undefined
	}
}
