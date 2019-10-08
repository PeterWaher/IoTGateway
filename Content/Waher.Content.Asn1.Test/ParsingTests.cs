using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.Asn1.Test
{
	[TestClass]
	public class ParsingTests
	{
		public static Asn1Document ParseAsn1Document(string FileName)
		{
			string Text = File.ReadAllText(Path.Combine("Examples", FileName));
			Asn1Document Doc = new Asn1Document(Text);
			return Doc;
		}

		[TestMethod]
		public void Test_01_WorldSchema()
		{
			ParseAsn1Document("World-Schema.asn1");
		}

		[TestMethod]
		public void Test_02_MyShopPurchaseOrders()
		{
			ParseAsn1Document("MyShopPurchaseOrders.asn1");
		}

		[TestMethod]
		public void Test_03_RFC1155()
		{
			ParseAsn1Document("SNMPv1\\RFC1155-SMI.asn1");
		}

		[TestMethod]
		public void Test_04_RFC1157()
		{
			ParseAsn1Document("SNMPv1\\RFC1157-SNMP.asn1");
		}
	}
}
