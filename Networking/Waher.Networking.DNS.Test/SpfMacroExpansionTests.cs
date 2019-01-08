using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using Waher.Networking.DNS.SPF;
using Waher.Networking.DNS.SPF.Mechanisms;

namespace Waher.Networking.DNS.Test
{
	[TestClass]
	public class SpfMacroExpansionTests
	{
		private Term term;

		[TestInitialize]
		public void TestInitialize()
		{
			this.term = new Term("strong-bad@email.example.com",
				"email.example.com", IPAddress.Parse("192.0.2.3"),
				"email.example.com", "example.com");
		}

		private async Task Test(string Unexpanded, string ExpectedResult)
		{
			this.term.Reset(":" + Unexpanded);
			Exists Exists = new Exists(this.term, SpfQualifier.Pass);
			await Exists.Expand();
			Assert.AreEqual(ExpectedResult, Exists.Domain);
		}

		[TestMethod]
		public async Task Test_01_SPF_Macro_Sender()
		{
			await this.Test("%{s}", "strong-bad@email.example.com");
		}

		[TestMethod]
		public async Task Test_02_SPF_Macro_Domain_Of_Sender()
		{
			await this.Test("%{o}", "email.example.com");
		}

		[TestMethod]
		public async Task Test_03_SPF_Macro_Domain()
		{
			await this.Test("%{d}", "email.example.com");
		}

		[TestMethod]
		public async Task Test_04_SPF_Macro_Domain_4()
		{
			await this.Test("%{d4}", "email.example.com");
		}

		[TestMethod]
		public async Task Test_05_SPF_Macro_Domain_3()
		{
			await this.Test("%{d3}", "email.example.com");
		}

		[TestMethod]
		public async Task Test_06_SPF_Macro_Domain_2()
		{
			await this.Test("%{d2}", "example.com");
		}

		[TestMethod]
		public async Task Test_07_SPF_Macro_Domain_1()
		{
			await this.Test("%{d1}", "com");
		}

		[TestMethod]
		public async Task Test_08_SPF_Macro_Domain_Reverse()
		{
			await this.Test("%{dr}", "com.example.email");
		}

		[TestMethod]
		public async Task Test_09_SPF_Macro_Domain_Reverse_2()
		{
			await this.Test("%{d2r}", "example.email");
		}

		[TestMethod]
		public async Task Test_10_SPF_Macro_Local_Part_Of_Sender()
		{
			await this.Test("%{l}", "strong-bad");
		}

		[TestMethod]
		public async Task Test_11_SPF_Macro_Local_Part_Of_Sender_Delimiter()
		{
			await this.Test("%{l-}", "strong.bad");
		}

		[TestMethod]
		public async Task Test_12_SPF_Macro_Local_Part_Of_Sender_Reverse()
		{
			await this.Test("%{lr}", "strong-bad");
		}

		[TestMethod]
		public async Task Test_13_SPF_Macro_Local_Part_Of_Sender_Delimiter_Reverse()
		{
			await this.Test("%{lr-}", "bad.strong");
		}

		[TestMethod]
		public async Task Test_14_SPF_Macro_Local_Part_Of_Sender_1_Delimiter_Reverse()
		{
			await this.Test("%{l1r-}", "strong");
		}

		[TestMethod]
		public async Task Test_15_SPF_Macro_Multi_1()
		{
			await this.Test("%{ir}.%{v}._spf.%{d2}", "3.2.0.192.in-addr._spf.example.com");
		}

		[TestMethod]
		public async Task Test_16_SPF_Macro_Multi_2()
		{
			await this.Test("%{lr-}.lp._spf.%{d2}", "bad.strong.lp._spf.example.com");
		}

		[TestMethod]
		public async Task Test_17_SPF_Macro_Multi_3()
		{
			await this.Test("%{lr-}.lp.%{ir}.%{v}._spf.%{d2}", "bad.strong.lp.3.2.0.192.in-addr._spf.example.com");
		}

		[TestMethod]
		public async Task Test_18_SPF_Macro_Multi_4()
		{
			await this.Test("%{ir}.%{v}.%{l1r-}.lp._spf.%{d2}", "3.2.0.192.in-addr.strong.lp._spf.example.com");
		}

		[TestMethod]
		public async Task Test_19_SPF_Macro_Multi_5()
		{
			await this.Test("%{d2}.trusted-domains.example.net", "example.com.trusted-domains.example.net");
		}

		[TestMethod]
		public async Task Test_20_SPF_IP6()
		{
			this.term = new Term("strong-bad@email.example.com",
				"email.example.com", IPAddress.Parse("2001:db8::cb01"),
				"email.example.com", "example.com");

			await this.Test("%{ir}.%{v}._spf.%{d2}", "1.0.b.c.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.8.b.d.0.1.0.0.2.ip6._spf.example.com");
		}
	}
}
