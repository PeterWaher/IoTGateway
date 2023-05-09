using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Images;
using Waher.Content.Emoji.Emoji1;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Script;
using Waher.Script.Graphs;

namespace Waher.Content.Markdown.Test
{
	[TestClass]
	public class MarkdownPlainTextTests
	{
		private static FilesProvider filesProvider = null;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Waher.Runtime.Inventory.Types.Initialize(
				typeof(MarkdownPlainTextTests).Assembly,
				typeof(Expression).Assembly,
				typeof(Graph).Assembly,
				typeof(MarkdownDocument).Assembly,
				typeof(CommonTypes).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(ImageCodec).Assembly);

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			filesProvider?.Dispose();
			filesProvider = null;
		}

		private async Task DoTest(string MarkdownFileName, string PlainTextFileName)
		{
			string Markdown = await Resources.ReadAllTextAsync("Markdown/Syntax/" + MarkdownFileName);
			string ExpectedText = await Resources.ReadAllTextAsync("PlainText/" + PlainTextFileName);
			Emoji1LocalFiles Emoji1LocalFiles = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/emoji1/%FILENAME%", Path.Combine("Graphics", "Emoji1.zip"), "Graphics");

			MarkdownSettings Settings = new MarkdownSettings(Emoji1LocalFiles, true, new Variables())
			{
				HttpxProxy = "/HttpxProxy/%URL%"
			};

			Assert.IsTrue(Emoji1LocalFiles.WaitUntilInitialized(60000));

			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);
			string GeneratedText = await Doc.GeneratePlainText();

			Console.Out.WriteLine(GeneratedText);
			Console.Out.WriteLine();
			Console.Out.WriteLine();
			Console.Out.WriteLine();

			MarkdownHtmlTests.AssertEqual(ExpectedText, GeneratedText, "Generated Plain Text does not match expected Plain Text.");
		}

		[TestMethod]
		public async Task Test_01_Paragraphs()
		{
			await this.DoTest("Test_01_Paragraphs.md", "Test_01_Paragraphs.txt");
		}

		[TestMethod]
		public async Task Test_02_Links()
		{
			await this.DoTest("Test_02_Links.md", "Test_02_Links.txt");
		}

		[TestMethod]
		public async Task Test_03_TextFormatting()
		{
			await this.DoTest("Test_03_TextFormatting.md", "Test_03_TextFormatting.txt");
		}

		[TestMethod]
		public async Task Test_04_Multimedia()
		{
			await this.DoTest("Test_04_Multimedia.md", "Test_04_Multimedia.txt");
		}

		[TestMethod]
		public async Task Test_05_HTML()
		{
			await this.DoTest("Test_05_HTML.md", "Test_05_HTML.txt");
		}

		[TestMethod]
		public async Task Test_06_CodeBlocks()
		{
			await this.DoTest("Test_06_CodeBlocks.md", "Test_06_CodeBlocks.txt");
		}

		[TestMethod]
		public async Task Test_07_BlockQuotes()
		{
			await this.DoTest("Test_07_BlockQuotes.md", "Test_07_BlockQuotes.txt");
		}

		[TestMethod]
		public async Task Test_08_Headers()
		{
			await this.DoTest("Test_08_Headers.md", "Test_08_Headers.txt");
		}

		[TestMethod]
		public async Task Test_09_UnorderedLists()
		{
			await this.DoTest("Test_09_UnorderedLists.md", "Test_09_UnorderedLists.txt");
		}

		[TestMethod]
		public async Task Test_10_LazyOrderedLists()
		{
			await this.DoTest("Test_10_LazyOrderedLists.md", "Test_10_LazyOrderedLists.txt");
		}

		[TestMethod]
		public async Task Test_11_OrderedLists()
		{
			await this.DoTest("Test_11_OrderedLists.md", "Test_11_OrderedLists.txt");
		}

		[TestMethod]
		public async Task Test_12_Typography()
		{
			await this.DoTest("Test_12_Typography.md", "Test_12_Typography.txt");
		}

		[TestMethod]
		public async Task Test_13_Tables()
		{
			await this.DoTest("Test_13_Tables.md", "Test_13_Tables.txt");
		}

		[TestMethod]
		public async Task Test_14_HorizontalRules()
		{
			await this.DoTest("Test_14_HorizontalRules.md", "Test_14_HorizontalRules.txt");
		}

		[TestMethod]
		public async Task Test_15_DefinitionLists()
		{
			await this.DoTest("Test_15_DefinitionLists.md", "Test_15_DefinitionLists.txt");
		}

		[TestMethod]
		public async Task Test_16_MetaData()
		{
			await this.DoTest("Test_16_MetaData.md", "Test_16_MetaData.txt");
		}

		[TestMethod]
		public async Task Test_17_Footnotes()
		{
			await this.DoTest("Test_17_Footnotes.md", "Test_17_Footnotes.txt");
		}

		[TestMethod]
		public async Task Test_18_Emojis()
		{
			await this.DoTest("Test_18_Emojis.md", "Test_18_Emojis.txt");
		}

		[TestMethod]
		public async Task Test_19_Sections()
		{
			await this.DoTest("Test_19_Sections.md", "Test_19_Sections.txt");
		}

		[TestMethod]
		public async Task Test_20_Script()
		{
			await this.DoTest("Test_20_Script.md", "Test_20_Script.txt");
		}

		[TestMethod]
		public async Task Test_21_Httpx()
		{
			await this.DoTest("Test_21_Httpx.md", "Test_21_Httpx.txt");
		}

		[TestMethod]
		public async Task Test_22_TaskLists()
		{
			await this.DoTest("Test_22_TaskLists.md", "Test_22_TaskLists.txt");
		}

		[TestMethod]
		public async Task Test_23_Superscript()
		{
			await this.DoTest("Test_23_Superscript.md", "Test_23_Superscript.txt");
		}

		[TestMethod]
		public async Task Test_24_Subscript()
		{
			await this.DoTest("Test_24_Subscript.md", "Test_24_Subscript.txt");
		}

		[TestMethod]
		public async Task Test_25_HashTags()
		{
			await this.DoTest("Test_25_HashTags.md", "Test_25_HashTags.txt");
		}

		[TestMethod]
		public async Task Test_26_Comments()
		{
			await this.DoTest("Test_26_Comments.md", "Test_26_Comments.txt");
		}

		[TestMethod]
		public async Task Test_27_Contract()
		{
			await this.DoTest("Test_27_Contract.md", "Test_27_Contract.txt");
		}

		[TestMethod]
		public async Task Test_28_Nesting()
		{
			await this.DoTest("Test_28_Nesting.md", "Test_28_Nesting.txt");
		}

		[TestMethod]
		public async Task Test_29_Justification()
		{
			await this.DoTest("Test_29_Justification.md", "Test_29_Justification.txt");
		}
	}
}
