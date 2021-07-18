using System;

namespace Waher.Networking.WHOIS
{
	public partial class WhoIsClient
	{
		private static readonly string[] rdapServices = new string[]
		{
			"https://rdap.apnic.net/",
			"https://rdap.db.ripe.net/",
			"https://rdap.arin.net/registry/",
			"https://rdap.afrinic.net/rdap/",
			"https://rdap.lacnic.net/rdap/",
			string.Empty
		};
	}
}
