using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
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
		public static void AssemblyInitialize(TestContext _)
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

			filesProvider = new FilesProvider("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			if (filesProvider != null)
			{
				filesProvider.Dispose();
				filesProvider = null;
			}
		}

		private void DoTest(string MarkdownFileName, string PlainTextFileName)
		{
			string Markdown = File.ReadAllText("Markdown/Syntax/" + MarkdownFileName);
			string ExpectedText = File.ReadAllText("PlainText/" + PlainTextFileName);
			Emoji1LocalFiles Emoji1LocalFiles = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/emoji1/%FILENAME%", Path.Combine("Graphics", "Emoji1.zip"), "Graphics");

			MarkdownSettings Settings = new MarkdownSettings(Emoji1LocalFiles, true, new Variables())
			{
				HttpxProxy = "/HttpxProxy/%URL%"
			};

			Assert.IsTrue(Emoji1LocalFiles.WaitUntilInitialized(60000));

			MarkdownDocument Doc = new MarkdownDocument(Markdown, Settings);
			string GeneratedText = Doc.GeneratePlainText();

			Console.Out.WriteLine(GeneratedText);
			Console.Out.WriteLine();
			Console.Out.WriteLine();
			Console.Out.WriteLine();

			MarkdownHtmlTests.AssertEqual(ExpectedText, GeneratedText, "Generated Plain Text does not match expected Plain Text.");
		}

		[TestMethod]
		public void Test_01_Paragraphs()
		{
			this.DoTest("Test_01_Paragraphs.md", "Test_01_Paragraphs.txt");
		}

		[TestMethod]
		public void Test_02_Links()
		{
			this.DoTest("Test_02_Links.md", "Test_02_Links.txt");
		}

		[TestMethod]
		public void Test_03_TextFormatting()
		{
			this.DoTest("Test_03_TextFormatting.md", "Test_03_TextFormatting.txt");
		}

		[TestMethod]
		public void Test_04_Multimedia()
		{
			this.DoTest("Test_04_Multimedia.md", "Test_04_Multimedia.txt");
		}

		[TestMethod]
		public void Test_05_HTML()
		{
			this.DoTest("Test_05_HTML.md", "Test_05_HTML.txt");
		}

		[TestMethod]
		public void Test_06_CodeBlocks()
		{
			this.DoTest("Test_06_CodeBlocks.md", "Test_06_CodeBlocks.txt");
		}

		[TestMethod]
		public void Test_07_BlockQuotes()
		{
			this.DoTest("Test_07_BlockQuotes.md", "Test_07_BlockQuotes.txt");
		}

		[TestMethod]
		public void Test_08_Headers()
		{
			this.DoTest("Test_08_Headers.md", "Test_08_Headers.txt");
		}

		[TestMethod]
		public void Test_09_UnorderedLists()
		{
			this.DoTest("Test_09_UnorderedLists.md", "Test_09_UnorderedLists.txt");
		}

		[TestMethod]
		public void Test_10_LazyOrderedLists()
		{
			this.DoTest("Test_10_LazyOrderedLists.md", "Test_10_LazyOrderedLists.txt");
		}

		[TestMethod]
		public void Test_11_OrderedLists()
		{
			this.DoTest("Test_11_OrderedLists.md", "Test_11_OrderedLists.txt");
		}
		
		[TestMethod]
		public void Test_12_Typography()
		{
			this.DoTest("Test_12_Typography.md", "Test_12_Typography.txt");
		}
		
		[TestMethod]
		public void Test_13_Tables()
		{
			this.DoTest("Test_13_Tables.md", "Test_13_Tables.txt");
		}
		
		[TestMethod]
		public void Test_14_HorizontalRules()
		{
			this.DoTest("Test_14_HorizontalRules.md", "Test_14_HorizontalRules.txt");
		}

		[TestMethod]
		public void Test_15_DefinitionLists()
		{
			this.DoTest("Test_15_DefinitionLists.md", "Test_15_DefinitionLists.txt");
		}

		[TestMethod]
		public void Test_16_MetaData()
		{
			this.DoTest("Test_16_MetaData.md", "Test_16_MetaData.txt");
		}

		[TestMethod]
		public void Test_17_Footnotes()
		{
			this.DoTest("Test_17_Footnotes.md", "Test_17_Footnotes.txt");
		}

		[TestMethod]
		public void Test_18_Emojis()
		{
			this.DoTest("Test_18_Emojis.md", "Test_18_Emojis.txt");
		}

		[TestMethod]
		public void Test_19_Sections()
		{
			this.DoTest("Test_19_Sections.md", "Test_19_Sections.txt");
		}

        [TestMethod]
        public void Test_20_Script()
        {
            this.DoTest("Test_20_Script.md", "Test_20_Script.txt");
        }

		[TestMethod]
		public void Test_21_Httpx()
		{
			this.DoTest("Test_21_Httpx.md", "Test_21_Httpx.txt");
		}

		[TestMethod]
		public void Test_22_TaskLists()
		{
			this.DoTest("Test_22_TaskLists.md", "Test_22_TaskLists.txt");
		}

		[TestMethod]
		public void Test_23_Superscript()
		{
			this.DoTest("Test_23_Superscript.md", "Test_23_Superscript.txt");
		}

		[TestMethod]
		public void Test_24_Subscript()
		{
			this.DoTest("Test_24_Subscript.md", "Test_24_Subscript.txt");
		}

		[TestMethod]
		public void Test_25_HashTags()
		{
			this.DoTest("Test_25_HashTags.md", "Test_25_HashTags.txt");
		}
	}
}
