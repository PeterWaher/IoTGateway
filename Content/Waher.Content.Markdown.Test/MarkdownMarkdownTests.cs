using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Emoji.Emoji1;
using Waher.Script;
using System.Threading.Tasks;

namespace Waher.Content.Markdown.Test
{
	[TestClass]
	public class MarkdownMarkdownTests
	{
		private static async Task DoTest(string MarkdownFileName)
		{
			string Markdown = await Resources.ReadAllTextAsync("Markdown/Syntax/" + MarkdownFileName);
			string ExpectedText = await Resources.ReadAllTextAsync("Markdown/Generated/" + MarkdownFileName);
			Emoji1LocalFiles Emoji1LocalFiles = new(Emoji1SourceFileType.Svg, 24, 24, "/emoji1/%FILENAME%", Path.Combine("Graphics", "Emoji1.zip"), "Graphics");

			MarkdownSettings Settings = new(Emoji1LocalFiles, true, new Variables())
			{
				HttpxProxy = "/HttpxProxy/%URL%"
			};

			Assert.IsTrue(Emoji1LocalFiles.WaitUntilInitialized(60000));

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);
			string GeneratedMarkdown = await Doc.GenerateMarkdown();

			Console.Out.WriteLine(GeneratedMarkdown);
			Console.Out.WriteLine();
			Console.Out.WriteLine();
			Console.Out.WriteLine();

			MarkdownHtmlTests.AssertEqual(ExpectedText, GeneratedMarkdown, "Generated Markdown does not match expected Markdown.");
		}

		[TestMethod]
		public async Task Test_01_Paragraphs()
		{
			await DoTest("Test_01_Paragraphs.md");
		}

		[TestMethod]
		public async Task Test_02_Links()
		{
			await DoTest("Test_02_Links.md");
		}

		[TestMethod]
		public async Task Test_03_TextFormatting()
		{
			await DoTest("Test_03_TextFormatting.md");
		}

		[TestMethod]
		public async Task Test_04_Multimedia()
		{
			await DoTest("Test_04_Multimedia.md");
		}

		[TestMethod]
		public async Task Test_05_HTML()
		{
			await DoTest("Test_05_HTML.md");
		}

		[TestMethod]
		public async Task Test_06_CodeBlocks()
		{
			await DoTest("Test_06_CodeBlocks.md");
		}

		[TestMethod]
		public async Task Test_07_BlockQuotes()
		{
			await DoTest("Test_07_BlockQuotes.md");
		}

		[TestMethod]
		public async Task Test_08_Headers()
		{
			await DoTest("Test_08_Headers.md");
		}

		[TestMethod]
		public async Task Test_09_UnorderedLists()
		{
			await DoTest("Test_09_UnorderedLists.md");
		}

		[TestMethod]
		public async Task Test_10_LazyOrderedLists()
		{
			await DoTest("Test_10_LazyOrderedLists.md");
		}

		[TestMethod]
		public async Task Test_11_OrderedLists()
		{
			await DoTest("Test_11_OrderedLists.md");
		}

		[TestMethod]
		public async Task Test_12_Typography()
		{
			await DoTest("Test_12_Typography.md");
		}

		[TestMethod]
		public async Task Test_13_Tables()
		{
			await DoTest("Test_13_Tables.md");
		}

		[TestMethod]
		public async Task Test_14_HorizontalRules()
		{
			await DoTest("Test_14_HorizontalRules.md");
		}

		[TestMethod]
		public async Task Test_15_DefinitionLists()
		{
			await DoTest("Test_15_DefinitionLists.md");
		}

		[TestMethod]
		public async Task Test_16_MetaData()
		{
			await DoTest("Test_16_MetaData.md");
		}

		[TestMethod]
		public async Task Test_17_Footnotes()
		{
			await DoTest("Test_17_Footnotes.md");
		}

		[TestMethod]
		public async Task Test_18_Emojis()
		{
			await DoTest("Test_18_Emojis.md");
		}

		[TestMethod]
		public async Task Test_19_Sections()
		{
			await DoTest("Test_19_Sections.md");
		}

        [TestMethod]
        public async Task Test_20_Script()
        {
            await DoTest("Test_20_Script.md");
        }

		[TestMethod]
		public async Task Test_21_Httpx()
		{
			await DoTest("Test_21_Httpx.md");
		}

		[TestMethod]
		public async Task Test_22_TaskLists()
		{
			await DoTest("Test_22_TaskLists.md");
		}

		[TestMethod]
		public async Task Test_23_Superscript()
		{
			await DoTest("Test_23_Superscript.md");
		}

		[TestMethod]
		public async Task Test_24_Subscript()
		{
			await DoTest("Test_24_Subscript.md");
		}

		[TestMethod]
		public async Task Test_25_HashTags()
		{
			await DoTest("Test_25_HashTags.md");
		}

		[TestMethod]
		public async Task Test_26_Comments()
		{
			await DoTest("Test_26_Comments.md");
		}

		[TestMethod]
		public async Task Test_27_Contract()
		{
			await DoTest("Test_27_Contract.md");
		}

		[TestMethod]
		public async Task Test_28_Nesting()
		{
			await DoTest("Test_28_Nesting.md");
		}

		[TestMethod]
		public async Task Test_29_Justification()
		{
			await DoTest("Test_29_Justification.md");
		}

		[TestMethod]
		public async Task Test_30_Incomplete()
		{
			await DoTest("Test_30_Incomplete.md");
		}

		[TestMethod]
		public async Task Test_31_Justification2()
		{
			await DoTest("Test_31_Justification2.md");
		}

		[TestMethod]
		public async Task Test_32_TablesAndNotes()
		{
			await DoTest("Test_32_TablesAndNotes.md");
		}

		[TestMethod]
		public async Task Test_33_SingleNoHeaderTable()
		{
			await DoTest("Test_33_SingleNoHeaderTable.md");
		}
	}
}
