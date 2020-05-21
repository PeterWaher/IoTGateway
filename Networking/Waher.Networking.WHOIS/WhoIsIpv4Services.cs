using System;

namespace Waher.Networking.WHOIS
{
	public partial class WhoIsClient
	{
		private static readonly string[] ipv4WhoIsServices = new string[]
		{
			"whois.apnic.net",
			"whois.ripe.net",
			"whois.arin.net",
			"whois.afrinic.net",
			"whois.lacnic.net",
			string.Empty
		};
	}
}
