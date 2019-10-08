using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.Asn1.Test
{
	[TestClass]
	public class CSharpTests
	{
		public static string GenerateCSharp(string FileName)
		{
			Asn1Document Doc = ParsingTests.ParseAsn1Document(FileName);
			string CSharp = Doc.ExportCSharp("Test", new CSharpExportSettings());
			Console.Out.WriteLine(CSharp);
			return CSharp;
		}

		[TestMethod]
		public void Test_01_WorldSchema()
		{
			GenerateCSharp("World-Schema.asn1");
		}

		[TestMethod]
		public void Test_02_MyShopPurchaseOrders()
		{
			GenerateCSharp("MyShopPurchaseOrders.asn1");
		}

	}
}
