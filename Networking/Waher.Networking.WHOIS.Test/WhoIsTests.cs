using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using Waher.Runtime.Console;

namespace Waher.Networking.WHOIS.Test
{
	[TestClass]
	public class WhoIsTests
	{
		[TestMethod]
		public async Task Test_01_APNIC()
		{
			string s = await WhoIsClient.Query(IPAddress.Parse("172.217.20.35"));
			ConsoleOut.WriteLine(s);
		}

		[TestMethod]
		public async Task Test_02_RIPE()
		{
			string s = await WhoIsClient.Query(IPAddress.Parse("81.236.63.162"));
			ConsoleOut.WriteLine(s);
		}

		[TestMethod]
		public async Task Test_03_ARIN()
		{
			string s = await WhoIsClient.Query(IPAddress.Parse("69.192.66.35"));
			ConsoleOut.WriteLine(s);
		}

		[TestMethod]
		public async Task Test_04_AFRINIC()
		{
			string s = await WhoIsClient.Query(IPAddress.Parse("42.1.1.1"));
			ConsoleOut.WriteLine(s);
		}

		[TestMethod]
		public async Task Test_05_LACNIC()
		{
			string s = await WhoIsClient.Query(IPAddress.Parse("192.230.79.221"));
			ConsoleOut.WriteLine(s);
		}

	}
}
