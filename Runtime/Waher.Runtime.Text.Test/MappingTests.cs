using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Runtime.Text.Test
{
	[TestClass]
	public class MappingTests
	{
		private static HarmonizedTextMap wordStyleMaps;

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			wordStyleMaps = CreateWordStyleMaps();
		}

		private static HarmonizedTextMap CreateWordStyleMaps()
		{
			HarmonizedTextMap Result = new();

			Result.RegisterMapping(@"H(EAD(ING)?)?\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"RUBRIK\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"RUBRIEK\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"ÜBERSCHRIFT\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"OVERSKRIFT\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"TITRE\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"TITOLO\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"TÍTULO\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"ЗАГОЛОВОК\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"OTSIKKO\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"NAGŁÓWEK\s*(?'N'\d+)", "H{N}");
			Result.RegisterMapping(@"NADPIS\s*(?'N'\d+)", "H{N}");

			Result.RegisterMapping(@"NORMAL", "NORMAL");
			Result.RegisterMapping(@"SUBTITLE", "NORMAL");
			Result.RegisterMapping(@"BODY TEXT(\s+\d+)", "NORMAL");

			Result.RegisterMapping(@"TITLE", "TITLE");
			Result.RegisterMapping(@"BOOK\s+TITLE", "TITLE");

			Result.RegisterMapping(@"LIST PARAGRAPH", "UL");
			Result.RegisterMapping(@"LISTSTYCKE", "UL");

			Result.RegisterMapping(@"QUOTE", "QUOTE");
			Result.RegisterMapping(@"INTENSE\s+QUOTE", "QUOTE");

			return Result;
		}

		[DataTestMethod]
		[DataRow("HEADING 1", "H1")]
		[DataRow("HEAD2", "H2")]
		[DataRow("H3", "H3")]
		[DataRow("RUBRIK1", "H1")]
		[DataRow("RUBRIK2", "H2")]
		[DataRow("LISTSTYCKE", "UL")]
		public void Test_01_WordStyles(string Input, string Expected)
		{
			Assert.IsTrue(wordStyleMaps.TryMap(Input, out string Harmonized));
			Assert.AreEqual(Expected, Harmonized);
		}
	}
}
