using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.DNS.Communication;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Runtime.Console;

namespace Waher.Networking.DNS.Test
{
	public abstract class DnsClientTests
	{
		private DnsClient client;

		[TestInitialize]
		public void TestInitialize()
		{
			this.client = this.CreateClient();
		}

		protected abstract DnsClient CreateClient();

		[TestCleanup]
		public void TestCleanup()
		{
			this.client?.Dispose();
			this.client = null;
		}

		[TestMethod]
		public async Task Test_01_Standard_Query_A()
		{
			DnsMessage Message = await this.client.QueryAsync("google.com", QTYPE.A, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			Print(Message);
		}

		[TestMethod]
		public async Task Test_02_Standard_Query_AAAA()
		{
			DnsMessage Message = await this.client.QueryAsync("google.com", QTYPE.AAAA, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			Print(Message);
		}

		[TestMethod]
		public async Task Test_03_Standard_Query_NonExistantDomain()
		{
			DnsMessage Message = await this.client.QueryAsync("dettanamnfinnsinte.se", QTYPE.A, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			Print(Message);
		}

		[TestMethod]
		public async Task Test_04_Standard_Query_MX()
		{
			DnsMessage Message = await this.client.QueryAsync("hotmail.com", QTYPE.MX, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			Print(Message);
		}

		[TestMethod]
		public async Task Test_05_Standard_Query_SRV()
		{
			DnsMessage Message = await this.client.QueryAsync("_xmpp-client._tcp.jabber.org", QTYPE.SRV, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			Print(Message);
		}

		[TestMethod]
		public async Task Test_06_Standard_Query_Reverse_IP4_Lookup()
		{
			DnsMessage Message = await this.client.QueryAsync("172.217.21.174.IN-ADDR.ARPA", QTYPE.PTR, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			Print(Message);
		}

		[TestMethod]
		public async Task Test_06_Standard_Query_Reverse_IP6_Lookup()
		{
			IPAddress Addr = IPAddress.Parse("2a00:1450:400f:80a::200e");
			StringBuilder sb = new();
			byte[] Bin = Addr.GetAddressBytes();
			int i;
			byte b, b2;

			for (i = 15; i >= 0; i--)
			{
				b = Bin[i];
				b2 = (byte)(b & 15);
				if (b2 < 10)
					sb.Append((char)('0' + b2));
				else
					sb.Append((char)('A' + b2- 10));

				sb.Append('.');

				b2 = (byte)(b >> 4);
				if (b2 < 10)
					sb.Append((char)('0' + b2));
				else
					sb.Append((char)('A' + b2 - 10));

				sb.Append('.');
			}
			sb.Append("IP6.ARPA");
			DnsMessage Message = await this.client.QueryAsync(sb.ToString(), QTYPE.PTR, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			Print(Message);
		}

		private static void Print(DnsMessage Message)
		{
			ConsoleOut.WriteLine("ID: " + Message.ID);
			ConsoleOut.WriteLine("OpCode: " + Message.OpCode);
			ConsoleOut.WriteLine("AuthoritativeAnswer: " + Message.AuthoritativeAnswer);
			ConsoleOut.WriteLine("Truncation: " + Message.Truncation);
			ConsoleOut.WriteLine("RecursionDesired: " + Message.RecursionDesired);
			ConsoleOut.WriteLine("RecursionAvailable: " + Message.RecursionAvailable);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine("Questions");
			ConsoleOut.WriteLine(new string('=', 40));

			foreach (Question Q in Message.Questions)
				ConsoleOut.WriteLine(Q.ToString());

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine("Answer");
			ConsoleOut.WriteLine(new string('=', 40));

			foreach (ResourceRecord RR in Message.Answer)
				ConsoleOut.WriteLine(RR.ToString());

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine("Authority");
			ConsoleOut.WriteLine(new string('=', 40));

			foreach (ResourceRecord RR in Message.Authority)
				ConsoleOut.WriteLine(RR.ToString());

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine("Additional");
			ConsoleOut.WriteLine(new string('=', 40));

			foreach (ResourceRecord RR in Message.Additional)
				ConsoleOut.WriteLine(RR.ToString());
		}
	}
}
