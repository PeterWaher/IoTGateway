using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.DNS.Communication;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;

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
			this.Print(Message);
		}

		[TestMethod]
		public async Task Test_02_Standard_Query_AAAA()
		{
			DnsMessage Message = await this.client.QueryAsync("google.com", QTYPE.AAAA, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			this.Print(Message);
		}

		[TestMethod]
		public async Task Test_03_Standard_Query_NonExistantDomain()
		{
			DnsMessage Message = await this.client.QueryAsync("dettanamnfinnsinte.se", QTYPE.A, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			this.Print(Message);
		}

		[TestMethod]
		public async Task Test_04_Standard_Query_MX()
		{
			DnsMessage Message = await this.client.QueryAsync("hotmail.com", QTYPE.MX, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			this.Print(Message);
		}

		[TestMethod]
		public async Task Test_05_Standard_Query_SRV()
		{
			DnsMessage Message = await this.client.QueryAsync("_xmpp-client._tcp.jabber.org", QTYPE.SRV, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			this.Print(Message);
		}

		[TestMethod]
		public async Task Test_06_Standard_Query_Reverse_IP4_Lookup()
		{
			DnsMessage Message = await this.client.QueryAsync("172.217.21.174.IN-ADDR.ARPA", QTYPE.PTR, QCLASS.IN, null);
			Assert.IsTrue(Message.Response);
			this.Print(Message);
		}

		[TestMethod]
		public async Task Test_06_Standard_Query_Reverse_IP6_Lookup()
		{
			IPAddress Addr = IPAddress.Parse("2a00:1450:400f:80a::200e");
			StringBuilder sb = new StringBuilder();
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
			this.Print(Message);
		}

		private void Print(DnsMessage Message)
		{
			Console.Out.WriteLine("ID: " + Message.ID);
			Console.Out.WriteLine("OpCode: " + Message.OpCode);
			Console.Out.WriteLine("AuthoritativeAnswer: " + Message.AuthoritativeAnswer);
			Console.Out.WriteLine("Truncation: " + Message.Truncation);
			Console.Out.WriteLine("RecursionDesired: " + Message.RecursionDesired);
			Console.Out.WriteLine("RecursionAvailable: " + Message.RecursionAvailable);

			Console.Out.WriteLine();
			Console.Out.WriteLine("Questions");
			Console.Out.WriteLine(new string('=', 40));

			foreach (Question Q in Message.Questions)
				Console.Out.WriteLine(Q.ToString());

			Console.Out.WriteLine();
			Console.Out.WriteLine("Answer");
			Console.Out.WriteLine(new string('=', 40));

			foreach (ResourceRecord RR in Message.Answer)
				Console.Out.WriteLine(RR.ToString());

			Console.Out.WriteLine();
			Console.Out.WriteLine("Authority");
			Console.Out.WriteLine(new string('=', 40));

			foreach (ResourceRecord RR in Message.Authority)
				Console.Out.WriteLine(RR.ToString());

			Console.Out.WriteLine();
			Console.Out.WriteLine("Additional");
			Console.Out.WriteLine(new string('=', 40));

			foreach (ResourceRecord RR in Message.Additional)
				Console.Out.WriteLine(RR.ToString());
		}
	}
}
