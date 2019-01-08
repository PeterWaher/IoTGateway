using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Runtime.Inventory;

namespace Waher.Networking.DNS.Test
{
	[TestClass]
	public class DnsResolverTests
	{
		private static FilesProvider filesProvider = null;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext Context)
		{
			Types.Initialize(
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(DnsResolver).Assembly);

			filesProvider = new FilesProvider("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			filesProvider?.Dispose();
			filesProvider = null;
		}

		[TestMethod]
		public void Test_01_DNS_Server_Addresses()
		{
			foreach (IPAddress Address in DnsResolver.DnsServerAddresses)
				Console.Out.WriteLine(Address.ToString());
		}

		[TestMethod]
		public async Task Test_02_Resolve_IPv4()
		{
			IPAddress[] Addresses = await DnsResolver.LookupIP4Addresses("google.com");
			foreach (IPAddress Address in Addresses)
				Console.Out.WriteLine(Address);
		}

		[TestMethod]
		public async Task Test_03_Resolve_IPv6()
		{
			IPAddress[] Addresses = await DnsResolver.LookupIP6Addresses("google.com");
			foreach (IPAddress Address in Addresses)
				Console.Out.WriteLine(Address);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task Test_04_NonexistantName()
		{
			IPAddress[] Addresses = await DnsResolver.LookupIP4Addresses("dettanamnfinnsinte.se");
			foreach (IPAddress Address in Addresses)
				Console.Out.WriteLine(Address);
		}

		[TestMethod]
		public async Task Test_05_Resolve_Mail_Exchange()
		{
			await this.TestExchange("hotmail.com");
		}

		private async Task TestExchange(string Domain)
		{
			string[] ExchangeHosts = await DnsResolver.LookupMailExchange(Domain);
			foreach (string ExchangeHost in ExchangeHosts)
			{
				Console.Out.WriteLine(ExchangeHost);

				IPAddress[] Addresses = await DnsResolver.LookupIP4Addresses(ExchangeHost);
				foreach (IPAddress Address in Addresses)
					Console.Out.WriteLine(Address);
			}
		}

		[TestMethod]
		public async Task Test_06_Resolve_Mail_Exchange_2()
		{
			await this.TestExchange("gmail.com");
		}

		[TestMethod]
		public async Task Test_07_Resolve_Reverse_IP4_Lookup()
		{
			string[] DomainNames = await DnsResolver.LookupDomainName(IPAddress.Parse("172.217.21.174"));
			foreach (string DomainName in DomainNames)
				Console.Out.WriteLine(DomainName);
		}

		[TestMethod]
		public async Task Test_08_Resolve_Reverse_IP6_Lookup()
		{
			string[] DomainNames = await DnsResolver.LookupDomainName(IPAddress.Parse("2a00:1450:400f:80a::200e"));
			foreach (string DomainName in DomainNames)
				Console.Out.WriteLine(DomainName);
		}

		[TestMethod]
		public async Task Test_09_Resolve_Service_Endpoint()
		{
			SRV Endpoint = await DnsResolver.LookupServiceEndpoint("jabber.org", "xmpp-client", "tcp");
			Console.Out.WriteLine(Endpoint.ToString());
		}

		[TestMethod]
		public async Task Test_10_Resolve_Service_Endpoints()
		{
			SRV[] Endpoints = await DnsResolver.LookupServiceEndpoints("jabber.org", "xmpp-client", "tcp");
			foreach (SRV SRV in Endpoints)
				Console.Out.WriteLine(SRV.ToString());
		}

		[TestMethod]
		public async Task Test_11_International_Domain_Names()
		{
			IPAddress[] Addresses = await DnsResolver.LookupIP4Addresses("bücher.com");
			foreach (IPAddress Address in Addresses)
				Console.Out.WriteLine(Address);
		}

		[TestMethod]
		public async Task Test_12_Resolve_Mail_Exchange_3()
		{
			await this.TestExchange("waher.se");
		}

		[TestMethod]
		public async Task Test_13_Resolve_Mail_Exchange_4()
		{
			await this.TestExchange("extas.is");
		}

		[TestMethod]
		public async Task Test_14_Resolve_Mail_Exchange_5()
		{
			await this.TestExchange("littlesister.se");
		}

		[TestMethod]
		public async Task Test_15_Resolve_DNSBL_Lookup_OK_IP()
		{
			string[] Reasons = await DnsResolver.LookupBlackList(IPAddress.Parse("194.9.95.112"), "zen.spamhaus.org");
			Assert.IsNull(Reasons);
		}

		[TestMethod]
		public async Task Test_16_Resolve_DNSBL_Lookup_Spam_IP()
		{
			string[] Reasons = await DnsResolver.LookupBlackList(IPAddress.Parse("179.49.7.95"), "zen.spamhaus.org");
			Assert.IsNotNull(Reasons);
			foreach (string Reason in Reasons)
				Console.Out.WriteLine(Reason);
		}

		[TestMethod]
		public async Task Test_17_Resolve_Reverse_IP4_Lookup()
		{
			string[] DomainNames = await DnsResolver.LookupDomainName(IPAddress.Parse("90.224.165.60"));
			foreach (string DomainName in DomainNames)
				Console.Out.WriteLine(DomainName);
		}

		[TestMethod]
		public async Task Test_18_Resolve_TXT()
		{
			string[] Text = await DnsResolver.LookupText("hotmail.com");
			foreach (string Row in Text)
				Console.Out.WriteLine(Row);
		}

		[TestMethod]
		public async Task Test_19_Resolve_TXT_2()
		{
			string[] Text = await DnsResolver.LookupText("waher.se");
			foreach (string Row in Text)
				Console.Out.WriteLine(Row);
		}

		[TestMethod]
		public async Task Test_20_Resolve_TXT_3()
		{
			string[] Text = await DnsResolver.LookupText("extas.is");
			foreach (string Row in Text)
				Console.Out.WriteLine(Row);
		}
	}
}
