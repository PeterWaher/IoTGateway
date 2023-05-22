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
		private async Task DoTest(string MarkdownFileName)
		{
			string Markdown = await Resources.ReadAllTextAsync("Markdown/Syntax/" + MarkdownFileName);
			string ExpectedText = await Resources.ReadAllTextAsync("Markdown/Generated/" + MarkdownFileName);
			ExpectedText = ExpectedText.Replace("&#xD;\r", "&#xD;");
			Emoji1LocalFiles Emoji1LocalFiles = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/emoji1/%FILENAME%", Path.Combine("Graphics", "Emoji1.zip"), "Graphics");

			MarkdownSettings Settings = new MarkdownSettings(Emoji1LocalFiles, true, new Variables())
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
			await this.DoTest("Test_01_Paragraphs.md");
		}

		[TestMethod]
		public async Task Test_02_Links()
		{
			await this.DoTest("Test_02_Links.md");
		}

		[TestMethod]
		public async Task Test_03_TextFormatting()
		{
			await this.DoTest("Test_03_TextFormatting.md");
		}

		[TestMethod]
		public async Task Test_04_Multimedia()
		{
			await this.DoTest("Test_04_Multimedia.md");
		}

		[TestMethod]
		public async Task Test_05_HTML()
		{
			await this.DoTest("Test_05_HTML.md");
		}

		[TestMethod]
		public async Task Test_06_CodeBlocks()
		{
			await this.DoTest("Test_06_CodeBlocks.md");
		}

		[TestMethod]
		public async Task Test_07_BlockQuotes()
		{
			await this.DoTest("Test_07_BlockQuotes.md");
		}

		[TestMethod]
		public async Task Test_08_Headers()
		{
			await this.DoTest("Test_08_Headers.md");
		}

		[TestMethod]
		public async Task Test_09_UnorderedLists()
		{
			await this.DoTest("Test_09_UnorderedLists.md");
		}

		[TestMethod]
		public async Task Test_10_LazyOrderedLists()
		{
			await this.DoTest("Test_10_LazyOrderedLists.md");
		}

		[TestMethod]
		public async Task Test_11_OrderedLists()
		{
			await this.DoTest("Test_11_OrderedLists.md");
		}

		[TestMethod]
		public async Task Test_12_Typography()
		{
			await this.DoTest("Test_12_Typography.md");
		}

		[TestMethod]
		public async Task Test_13_Tables()
		{
			await this.DoTest("Test_13_Tables.md");
		}

		[TestMethod]
		public async Task Test_14_HorizontalRules()
		{
			await this.DoTest("Test_14_HorizontalRules.md");
		}

		[TestMethod]
		public async Task Test_15_DefinitionLists()
		{
			await this.DoTest("Test_15_DefinitionLists.md");
		}

		[TestMethod]
		public async Task Test_16_MetaData()
		{
			await this.DoTest("Test_16_MetaData.md");
		}

		[TestMethod]
		public async Task Test_17_Footnotes()
		{
			await this.DoTest("Test_17_Footnotes.md");
		}

		[TestMethod]
		public async Task Test_18_Emojis()
		{
			await this.DoTest("Test_18_Emojis.md");
		}

		[TestMethod]
		public async Task Test_19_Sections()
		{
			await this.DoTest("Test_19_Sections.md");
		}

        [TestMethod]
        public async Task Test_20_Script()
        {
            await this.DoTest("Test_20_Script.md");
        }

		[TestMethod]
		public async Task Test_21_Httpx()
		{
			await this.DoTest("Test_21_Httpx.md");
		}

		[TestMethod]
		public async Task Test_22_TaskLists()
		{
			await this.DoTest("Test_22_TaskLists.md");
		}

		[TestMethod]
		public async Task Test_23_Superscript()
		{
			await this.DoTest("Test_23_Superscript.md");
		}

		[TestMethod]
		public async Task Test_24_Subscript()
		{
			await this.DoTest("Test_24_Subscript.md");
		}

		[TestMethod]
		public async Task Test_25_HashTags()
		{
			await this.DoTest("Test_25_HashTags.md");
		}

		[TestMethod]
		public async Task Test_26_Comments()
		{
			await this.DoTest("Test_26_Comments.md");
		}

		[TestMethod]
		public async Task Test_27_Contract()
		{
			await this.DoTest("Test_27_Contract.md");
		}

		[TestMethod]
		public async Task Test_28_Nesting()
		{
			await this.DoTest("Test_28_Nesting.md");
		}

		[TestMethod]
		public async Task Test_29_Justification()
		{
			await this.DoTest("Test_29_Justification.md");
		}

		[TestMethod]
		public async Task Test_30_Incomplete()
		{
			await this.DoTest("Test_30_Incomplete.md");
		}
	}
}
