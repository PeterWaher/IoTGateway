using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

namespace Waher.Networking.DNS.Test
{
	[TestClass]
	public class DnsResolverTests
	{
		[ClassInitialize]
		public static void ClassInitialize(TestContext Context)
		{
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
		}

		[TestMethod]
		public void Test_01_DNS_Server_Addresses()
		{
			foreach (IPAddress Address in DnsResolver.DnsServerAddresses)
				Console.Out.WriteLine(Address.ToString());
		}
	}
}
