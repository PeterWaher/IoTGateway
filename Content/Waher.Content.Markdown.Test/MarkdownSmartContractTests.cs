using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Waher.Content.Markdown.Contracts;
using Waher.Content.Xml;
using Waher.Runtime.Console;
using Waher.Script;

namespace Waher.Content.Markdown.Test
{
	[TestClass]
	public class MarkdownSmartContractTests
	{
		private static async Task DoTest(string MarkdownFileName, string XamlFileName)
		{
			string Markdown = await Resources.ReadAllTextAsync("Markdown/Syntax/" + MarkdownFileName);
			string ExpectedText = await Resources.ReadAllTextAsync("SC/" + XamlFileName);
			ExpectedText = ExpectedText.Replace("&#xD;\r", "&#xD;");

			MarkdownSettings Settings = new(null, true, new Variables());

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);
			string GeneratedXaml = await Doc.GenerateSmartContractXml(XML.WriterSettings(true, true));

			ConsoleOut.WriteLine(GeneratedXaml);
			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			MarkdownHtmlTests.AssertEqual(ExpectedText, GeneratedXaml, "Generated XML does not match expected XML.");
		}

		[TestMethod]
		public async Task Test_01_Paragraphs()
		{
			await DoTest("Test_01_Paragraphs.md", "Test_01_Paragraphs.xml");
		}

		[TestMethod]
		public async Task Test_02_Links()
		{
			await DoTest("Test_02_Links.md", "Test_02_Links.xml");
		}

		[TestMethod]
		public async Task Test_03_TextFormatting()
		{
			await DoTest("Test_03_TextFormatting.md", "Test_03_TextFormatting.xml");
		}

		[TestMethod]
		public async Task Test_04_Multimedia()
		{
			await DoTest("Test_04_Multimedia.md", "Test_04_Multimedia.xml");
		}

		[TestMethod]
		public async Task Test_05_HTML()
		{
			await DoTest("Test_05_HTML.md", "Test_05_HTML.xml");
		}

		[TestMethod]
		public async Task Test_06_CodeBlocks()
		{
			await DoTest("Test_06_CodeBlocks.md", "Test_06_CodeBlocks.xml");
		}

		[TestMethod]
		public async Task Test_07_BlockQuotes()
		{
			await DoTest("Test_07_BlockQuotes.md", "Test_07_BlockQuotes.xml");
		}

		[TestMethod]
		public async Task Test_08_Headers()
		{
			await DoTest("Test_08_Headers.md", "Test_08_Headers.xml");
		}

		[TestMethod]
		public async Task Test_09_UnorderedLists()
		{
			await DoTest("Test_09_UnorderedLists.md", "Test_09_UnorderedLists.xml");
		}

		[TestMethod]
		public async Task Test_10_LazyOrderedLists()
		{
			await DoTest("Test_10_LazyOrderedLists.md", "Test_10_LazyOrderedLists.xml");
		}

		[TestMethod]
		public async Task Test_11_OrderedLists()
		{
			await DoTest("Test_11_OrderedLists.md", "Test_11_OrderedLists.xml");
		}

		[TestMethod]
		public async Task Test_12_Typography()
		{
			await DoTest("Test_12_Typography.md", "Test_12_Typography.xml");
		}

		[TestMethod]
		public async Task Test_13_Tables()
		{
			await DoTest("Test_13_Tables.md", "Test_13_Tables.xml");
		}

		[TestMethod]
		public async Task Test_14_HorizontalRules()
		{
			await DoTest("Test_14_HorizontalRules.md", "Test_14_HorizontalRules.xml");
		}

		[TestMethod]
		public async Task Test_15_DefinitionLists()
		{
			await DoTest("Test_15_DefinitionLists.md", "Test_15_DefinitionLists.xml");
		}

		[TestMethod]
		public async Task Test_16_MetaData()
		{
			await DoTest("Test_16_MetaData.md", "Test_16_MetaData.xml");
		}

		[TestMethod]
		public async Task Test_17_Footnotes()
		{
			await DoTest("Test_17_Footnotes.md", "Test_17_Footnotes.xml");
		}

		[TestMethod]
		public async Task Test_18_Emojis()
		{
			await DoTest("Test_18_Emojis.md", "Test_18_Emojis.xml");
		}

		[TestMethod]
		public async Task Test_19_Sections()
		{
			await DoTest("Test_19_Sections.md", "Test_19_Sections.xml");
		}

        [TestMethod]
        public async Task Test_20_Script()
        {
            await DoTest("Test_20_Script.md", "Test_20_Script.xml");
        }

		[TestMethod]
		public async Task Test_21_Httpx()
		{
			await DoTest("Test_21_Httpx.md", "Test_21_Httpx.xml");
		}

		[TestMethod]
		public async Task Test_22_TaskLists()
		{
			await DoTest("Test_22_TaskLists.md", "Test_22_TaskLists.xml");
		}

		[TestMethod]
		public async Task Test_23_Superscript()
		{
			await DoTest("Test_23_Superscript.md", "Test_23_Superscript.xml");
		}

		[TestMethod]
		public async Task Test_24_Subscript()
		{
			await DoTest("Test_24_Subscript.md", "Test_24_Subscript.xml");
		}

		[TestMethod]
		public async Task Test_25_HashTags()
		{
			await DoTest("Test_25_HashTags.md", "Test_25_HashTags.xml");
		}

		[TestMethod]
		public async Task Test_26_Comments()
		{
			await DoTest("Test_26_Comments.md", "Test_26_Comments.xml");
		}

		[TestMethod]
		public async Task Test_27_Contract()
		{
			await DoTest("Test_27_Contract.md", "Test_27_Contract.xml");
		}

		[TestMethod]
		public async Task Test_28_Nesting()
		{
			await DoTest("Test_28_Nesting.md", "Test_28_Nesting.xml");
		}

		[TestMethod]
		public async Task Test_29_Justification()
		{
			await DoTest("Test_29_Justification.md", "Test_29_Justification.xml");
		}

		[TestMethod]
		public async Task Test_30_Incomplete()
		{
			await DoTest("Test_30_Incomplete.md", "Test_30_Incomplete.xml");
		}

		[TestMethod]
		public async Task Test_31_Justification2()
		{
			await DoTest("Test_31_Justification2.md", "Test_31_Justification2.xml");
		}

		[TestMethod]
		public async Task Test_32_TablesAndNotes()
		{
			await DoTest("Test_32_TablesAndNotes.md", "Test_32_TablesAndNotes.xml");
		}

		[TestMethod]
		public async Task Test_33_SingleNoHeaderTable()
		{
			await DoTest("Test_33_SingleNoHeaderTable.md", "Test_33_SingleNoHeaderTable.xml");
		}

        [TestMethod]
        public async Task Test_34_SpecifiedViewportHeader()
        {
            await DoTest("Test_34_SpecifiedViewportHeader.md", "Test_34_SpecifiedViewportHeader.xml");
        }
    }
}
