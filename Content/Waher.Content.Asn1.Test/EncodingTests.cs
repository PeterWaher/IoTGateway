using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.Asn1.Test
{
	[TestClass]
	public class EncodingTests
	{
		[TestMethod]
		public void Test_01_WorldSchema_CSharp()
		{
			Asn1Document Doc = ParsingTests.ParseAsn1Document("World-Schema.asn1");
			string CSharp = Doc.ExportCSharp("Test", new CSharpExportSettings());
			Console.Out.WriteLine(CSharp);
		}

	}
}
