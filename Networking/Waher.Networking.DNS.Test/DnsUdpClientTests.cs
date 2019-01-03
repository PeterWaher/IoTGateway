using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;
using Waher.Networking.DNS.Communication;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;

namespace Waher.Networking.DNS.Test
{
	[TestClass]
	public class DnsUdpClientTests
	{
		private DnsUdpClient client;

		[TestInitialize]
		public void TestInitialize()
		{
			this.client = new DnsUdpClient();
		}

		[TestCleanup]
		public void TestCleanup()
		{
			this.client?.Dispose();
			this.client = null;
		}

		[TestMethod]
		public async Task Test_01_Standard_Query_A()
		{
			DnsMessage Message = await this.client.QueryAsync("waher.se", QTYPE.A, QCLASS.IN);
			Assert.IsTrue(Message.Response);
			this.Print(Message);
		}

		[TestMethod]
		public async Task Test_02_Standard_Query_NonExistantDomain()
		{
			DnsMessage Message = await this.client.QueryAsync("dettanamnfinnsinte.se", QTYPE.A, QCLASS.IN);
			Assert.IsTrue(Message.Response);
			this.Print(Message);
		}

		[TestMethod]
		public async Task Test_03_Standard_Query_MX()
		{
			DnsMessage Message = await this.client.QueryAsync("hotmail.com", QTYPE.MX, QCLASS.IN);
			Assert.IsTrue(Message.Response);
			this.Print(Message);
		}

		[TestMethod]
		public async Task Test_04_Standard_Query_SRV()
		{
			DnsMessage Message = await this.client.QueryAsync("_xmpp-client._tcp.jabber.org", QTYPE.SRV, QCLASS.IN);
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
