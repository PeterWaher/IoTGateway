using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.Asn1.Test
{
	[TestClass]
	public class ParsingTests
	{
		private Asn1Document ParseAsn1Document(string FileName)
		{
			string Text = File.ReadAllText(Path.Combine("Examples", FileName));
			Asn1Document Doc = new Asn1Document(Text);
			return Doc;
		}

		[TestMethod]
		public void Test_01_MyShopPurchaseOrders()
		{
			this.ParseAsn1Document("MyShopPurchaseOrders.asn1");
		}
	}
}
