using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Waher.Runtime.Text.Test
{
	[TestClass]
	public class MappingTests
	{
		private static HarmonizedTextMap maps;

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			maps = new HarmonizedTextMap();

			maps.RegisterMapping(@"H(EAD(ING)?)?\s*(?'N'\d+)", "H{N}");
			maps.RegisterMapping(@"RUBRIK\s*(?'N'\d+)", "H{N}");
			maps.RegisterMapping(@"RUBRIEK\s*(?'N'\d+)", "H{N}");
			maps.RegisterMapping(@"ÜBERSCHRIFT\s*(?'N'\d+)", "H{N}");
			maps.RegisterMapping(@"OVERSKRIFT\s*(?'N'\d+)", "H{N}");
			maps.RegisterMapping(@"TITRE\s*(?'N'\d+)", "H{N}");
			maps.RegisterMapping(@"TITOLO\s*(?'N'\d+)", "H{N}");
			maps.RegisterMapping(@"TÍTULO\s*(?'N'\d+)", "H{N}");
			maps.RegisterMapping(@"ЗАГОЛОВОК\s*(?'N'\d+)", "H{N}");
			maps.RegisterMapping(@"OTSIKKO\s*(?'N'\d+)", "H{N}");
			maps.RegisterMapping(@"NAGŁÓWEK\s*(?'N'\d+)", "H{N}");
			maps.RegisterMapping(@"NADPIS\s*(?'N'\d+)", "H{N}");

			maps.RegisterMapping(@"NORMAL", "NORMAL");
			maps.RegisterMapping(@"SUBTITLE", "NORMAL");
			maps.RegisterMapping(@"BODY TEXT(\s+\d+)", "NORMAL");

			maps.RegisterMapping(@"TITLE", "TITLE");
			maps.RegisterMapping(@"BOOK\s+TITLE", "TITLE");

			maps.RegisterMapping(@"LIST PARAGRAPH", "UL");
			maps.RegisterMapping(@"LISTSTYCKE", "UL");

			maps.RegisterMapping(@"QUOTE", "QUOTE");
			maps.RegisterMapping(@"INTENSE\s+QUOTE", "QUOTE");
		}

		[DataTestMethod]
		[DataRow("HEADING 1", "H1")]
		[DataRow("HEAD2", "H2")]
		[DataRow("H3", "H3")]
		[DataRow("RUBRIK1", "H1")]
		[DataRow("RUBRIK2", "H2")]
		[DataRow("LISTSTYCKE", "UL")]
		public void Test_08_WordStyles(string Input, string Expected)
		{
			Assert.IsTrue(maps.TryMap(Input, out string Harmonized));
			Assert.AreEqual(Expected, Harmonized);
		}
	}
}
