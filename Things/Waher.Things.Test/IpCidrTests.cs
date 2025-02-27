using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using Waher.Runtime.IO;

namespace Waher.Things.Test
{
	[TestClass]
	public class IpCidrTests
	{
		[TestMethod]
		public void Test_01_IpAddress()
		{
			Assert.IsTrue(IpCidr.TryParse("1.2.3.4", out IpCidr Parsed));
			Assert.AreEqual(IPAddress.Parse("1.2.3.4"), Parsed.Address);
			Assert.AreEqual(32, Parsed.Range);
			Assert.IsTrue(Parsed.Matches(IPAddress.Parse("1.2.3.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("2.2.3.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("1.3.3.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("1.2.4.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("1.2.3.5")));
		}

		[TestMethod]
		public void Test_02_IpRange_24()
		{
			Assert.IsTrue(IpCidr.TryParse("1.2.3.4/24", out IpCidr Parsed));
			Assert.AreEqual(IPAddress.Parse("1.2.3.4"), Parsed.Address);
			Assert.AreEqual(24, Parsed.Range);
			Assert.IsTrue(Parsed.Matches(IPAddress.Parse("1.2.3.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("2.2.3.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("1.3.3.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("1.2.4.4")));
			Assert.IsTrue(Parsed.Matches(IPAddress.Parse("1.2.3.5")));
		}

		[TestMethod]
		public void Test_03_IpRange_28()
		{
			Assert.IsTrue(IpCidr.TryParse("1.2.3.4/28", out IpCidr Parsed));
			Assert.AreEqual(IPAddress.Parse("1.2.3.4"), Parsed.Address);
			Assert.AreEqual(28, Parsed.Range);
			Assert.IsTrue(Parsed.Matches(IPAddress.Parse("1.2.3.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("2.2.3.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("1.3.3.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("1.2.4.4")));
			Assert.IsTrue(Parsed.Matches(IPAddress.Parse("1.2.3.15")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("1.2.3.16")));
		}

		[TestMethod]
		public void Test_04_IpRange_10()
		{
			Assert.IsTrue(IpCidr.TryParse("1.2.3.4/10", out IpCidr Parsed));
			Assert.AreEqual(IPAddress.Parse("1.2.3.4"), Parsed.Address);
			Assert.AreEqual(10, Parsed.Range);
			Assert.IsTrue(Parsed.Matches(IPAddress.Parse("1.2.3.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("2.2.3.4")));
			Assert.IsTrue(Parsed.Matches(IPAddress.Parse("1.3.3.4")));
			Assert.IsTrue(Parsed.Matches(IPAddress.Parse("1.63.3.4")));
			Assert.IsFalse(Parsed.Matches(IPAddress.Parse("1.64.3.4")));
			Assert.IsTrue(Parsed.Matches(IPAddress.Parse("1.2.4.4")));
			Assert.IsTrue(Parsed.Matches(IPAddress.Parse("1.2.3.255")));
		}
	}
}
