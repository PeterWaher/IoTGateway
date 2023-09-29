using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Xml;
using Waher.Script;
using System.Threading.Tasks;

namespace Waher.Content.Markdown.Test
{
	[TestClass]
	public class MarkdownSmartContractTests
	{
		private async Task DoTest(string MarkdownFileName, string XamlFileName)
		{
			string Markdown = await Resources.ReadAllTextAsync("Markdown/Syntax/" + MarkdownFileName);
			string ExpectedText = await Resources.ReadAllTextAsync("SC/" + XamlFileName);
			ExpectedText = ExpectedText.Replace("&#xD;\r", "&#xD;");

			MarkdownSettings Settings = new MarkdownSettings(null, true, new Variables());

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);
			string GeneratedXaml = await Doc.GenerateSmartContractXml(XML.WriterSettings(true, true));

			Console.Out.WriteLine(GeneratedXaml);
			Console.Out.WriteLine();
			Console.Out.WriteLine();
			Console.Out.WriteLine();

			MarkdownHtmlTests.AssertEqual(ExpectedText, GeneratedXaml, "Generated XML does not match expected XML.");
		}

		[TestMethod]
		public async Task Test_01_Paragraphs()
		{
			await this.DoTest("Test_01_Paragraphs.md", "Test_01_Paragraphs.xml");
		}

		[TestMethod]
		public async Task Test_02_Links()
		{
			await this.DoTest("Test_02_Links.md", "Test_02_Links.xml");
		}

		[TestMethod]
		public async Task Test_03_TextFormatting()
		{
			await this.DoTest("Test_03_TextFormatting.md", "Test_03_TextFormatting.xml");
		}

		[TestMethod]
		public async Task Test_04_Multimedia()
		{
			await this.DoTest("Test_04_Multimedia.md", "Test_04_Multimedia.xml");
		}

		[TestMethod]
		public async Task Test_05_HTML()
		{
			await this.DoTest("Test_05_HTML.md", "Test_05_HTML.xml");
		}

		[TestMethod]
		public async Task Test_06_CodeBlocks()
		{
			await this.DoTest("Test_06_CodeBlocks.md", "Test_06_CodeBlocks.xml");
		}

		[TestMethod]
		public async Task Test_07_BlockQuotes()
		{
			await this.DoTest("Test_07_BlockQuotes.md", "Test_07_BlockQuotes.xml");
		}

		[TestMethod]
		public async Task Test_08_Headers()
		{
			await this.DoTest("Test_08_Headers.md", "Test_08_Headers.xml");
		}

		[TestMethod]
		public async Task Test_09_UnorderedLists()
		{
			await this.DoTest("Test_09_UnorderedLists.md", "Test_09_UnorderedLists.xml");
		}

		[TestMethod]
		public async Task Test_10_LazyOrderedLists()
		{
			await this.DoTest("Test_10_LazyOrderedLists.md", "Test_10_LazyOrderedLists.xml");
		}

		[TestMethod]
		public async Task Test_11_OrderedLists()
		{
			await this.DoTest("Test_11_OrderedLists.md", "Test_11_OrderedLists.xml");
		}

		[TestMethod]
		public async Task Test_12_Typography()
		{
			await this.DoTest("Test_12_Typography.md", "Test_12_Typography.xml");
		}

		[TestMethod]
		public async Task Test_13_Tables()
		{
			await this.DoTest("Test_13_Tables.md", "Test_13_Tables.xml");
		}

		[TestMethod]
		public async Task Test_14_HorizontalRules()
		{
			await this.DoTest("Test_14_HorizontalRules.md", "Test_14_HorizontalRules.xml");
		}

		[TestMethod]
		public async Task Test_15_DefinitionLists()
		{
			await this.DoTest("Test_15_DefinitionLists.md", "Test_15_DefinitionLists.xml");
		}

		[TestMethod]
		public async Task Test_16_MetaData()
		{
			await this.DoTest("Test_16_MetaData.md", "Test_16_MetaData.xml");
		}

		[TestMethod]
		public async Task Test_17_Footnotes()
		{
			await this.DoTest("Test_17_Footnotes.md", "Test_17_Footnotes.xml");
		}

		[TestMethod]
		public async Task Test_18_Emojis()
		{
			await this.DoTest("Test_18_Emojis.md", "Test_18_Emojis.xml");
		}

		[TestMethod]
		public async Task Test_19_Sections()
		{
			await this.DoTest("Test_19_Sections.md", "Test_19_Sections.xml");
		}

        [TestMethod]
        public async Task Test_20_Script()
        {
            await this.DoTest("Test_20_Script.md", "Test_20_Script.xml");
        }

		[TestMethod]
		public async Task Test_21_Httpx()
		{
			await this.DoTest("Test_21_Httpx.md", "Test_21_Httpx.xml");
		}

		[TestMethod]
		public async Task Test_22_TaskLists()
		{
			await this.DoTest("Test_22_TaskLists.md", "Test_22_TaskLists.xml");
		}

		[TestMethod]
		public async Task Test_23_Superscript()
		{
			await this.DoTest("Test_23_Superscript.md", "Test_23_Superscript.xml");
		}

		[TestMethod]
		public async Task Test_24_Subscript()
		{
			await this.DoTest("Test_24_Subscript.md", "Test_24_Subscript.xml");
		}

		[TestMethod]
		public async Task Test_25_HashTags()
		{
			await this.DoTest("Test_25_HashTags.md", "Test_25_HashTags.xml");
		}

		[TestMethod]
		public async Task Test_26_Comments()
		{
			await this.DoTest("Test_26_Comments.md", "Test_26_Comments.xml");
		}

		[TestMethod]
		public async Task Test_27_Contract()
		{
			await this.DoTest("Test_27_Contract.md", "Test_27_Contract.xml");
		}

		[TestMethod]
		public async Task Test_28_Nesting()
		{
			await this.DoTest("Test_28_Nesting.md", "Test_28_Nesting.xml");
		}

		[TestMethod]
		public async Task Test_29_Justification()
		{
			await this.DoTest("Test_29_Justification.md", "Test_29_Justification.xml");
		}

		[TestMethod]
		public async Task Test_30_Incomplete()
		{
			await this.DoTest("Test_30_Incomplete.md", "Test_30_Incomplete.xml");
		}

		[TestMethod]
		public async Task Test_31_Justification2()
		{
			await this.DoTest("Test_31_Justification2.md", "Test_31_Justification2.xml");
		}
	}
}
